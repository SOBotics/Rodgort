using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Services;
using Rodgort.Utilities;
using StackExchangeChat;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly RodgortContext _context;
        private readonly BurnProcessingService _burnProcessingService;
        private readonly DateService _dateService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdminController> _logger;
        private readonly IApplicationLifetime _appLifetime;
        private readonly IConfiguration _configuration;

        public AdminController(RodgortContext context, BurnProcessingService burnProcessingService, DateService dateService, IServiceProvider serviceProvider, ILogger<AdminController> logger, IApplicationLifetime appLifetime, IConfiguration configuration)
        {
            _context = context;
            _burnProcessingService = burnProcessingService;
            _dateService = dateService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _appLifetime = appLifetime;
            _configuration = configuration;
        }

        [HttpGet("UnresolvedDeletions")]
        public object GetUnresolvedDeletions()
        {
            var isAdmin = User.HasRole(DbRole.ADMIN);
            if (!isAdmin)
                throw new HttpStatusException(HttpStatusCode.Unauthorized);

            return _context.UnknownDeletions
                .Where(ud => !ud.Processed.HasValue)
                .Select(ua => new
                {
                    UnknownDeletionId = ua.Id,
                    ua.PostId
                })
                .ToList();
        }

        [HttpPost("Shutdown")]
        public void Shutdown()
        {
            var isAdmin = User.HasRole(DbRole.ADMIN);
            if (!isAdmin)
                throw new HttpStatusException(HttpStatusCode.Unauthorized);

            _appLifetime.StopApplication();
        }

        [HttpPost("ResolveUnresolvedDeletion")]
        public object ResolveUnresolvedDeletion([FromBody] List<ResolveUnresolvedDeletionRequest> requestItems)
        {
            var isAdmin = User.HasRole(DbRole.ADMIN);
            if (!isAdmin)
                throw new HttpStatusException(HttpStatusCode.Unauthorized);

            foreach (var requestItemsPerAction in requestItems.GroupBy(g => g.UnknownDeletionId))
            {
                var matchedUnknown = _context.UnknownDeletions.FirstOrDefault(ua => ua.Id == requestItemsPerAction.Key);
                if (matchedUnknown == null)
                    continue;

                foreach (var requestItem in requestItemsPerAction)
                {
                    var hasMatchingFollow = _context.BurnakiFollows.Any(bf =>
                        bf.FollowStarted <= requestItem.DateTime
                        && (!bf.FollowEnded.HasValue || bf.FollowEnded >= requestItem.DateTime
                        ));

                    if (!hasMatchingFollow)
                        continue;

                    var tag = requestItem.Tag ?? matchedUnknown.Tag;

                    AddIfNew(new DbUserAction
                    {
                        PostId = matchedUnknown.PostId,
                        Tag = tag,
                        SiteUserId = requestItem.UserId,
                        Time = requestItem.DateTime,
                        UserActionTypeId = requestItem.ActionTypeId,
                        TimeProcessed = _dateService.UtcNow,
                        UnknownDeletionId = requestItemsPerAction.Key
                    });


                    void AddIfNew(DbUserAction action)
                    {
                        if (!_context.UserActions.Any(
                            ua => ua.UserActionTypeId == action.UserActionTypeId
                                  && ua.Tag == action.Tag
                                  && ua.PostId == action.PostId
                                  && ua.Time == action.Time
                                  && ua.SiteUserId == action.SiteUserId
                        ))
                        {
                            _context.UserActions.Add(action);
                        }
                    }
                }

                matchedUnknown.Processed = _dateService.UtcNow;
                matchedUnknown.ProcessedByUserId = User.UserId();
            }

            var userActionTagLookup = _context.UserActions.Local.Select(mqmt => new { mqmt.PostId, mqmt.Tag })
                .GroupBy(ua => ua.PostId)
                .ToDictionary(g => g.Key, g => g.GroupBy(gg => new { gg.PostId, gg.Tag }).Select(gg => gg.Key).ToList());

            var currentPostIds = userActionTagLookup.Keys.ToList();
            var dbSeenQuestionsLookup = _context.SeenQuestions.Where(t => currentPostIds.Contains(t.Id))
                .ToList()
                .GroupBy(t => t.Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var currentPostId in currentPostIds)
            {
                var dbSeenQuestions =
                    dbSeenQuestionsLookup.ContainsKey(currentPostId)
                        ? dbSeenQuestionsLookup[currentPostId]
                        : new List<DbSeenQuestion>();

                foreach (var userActionTag in userActionTagLookup[currentPostId])
                {
                    var matched = dbSeenQuestions.FirstOrDefault(dsq => dsq.Id == userActionTag.PostId && dsq.Tag == userActionTag.Tag);
                    if (matched == null)
                        _context.SeenQuestions.Add(new DbSeenQuestion { Id = currentPostId, LastSeen = _dateService.UtcNow, Tag = userActionTag.Tag });
                    else
                        matched.LastSeen = _dateService.UtcNow;
                }
            }

            var currentUserIds = _context.UserActions.Local.Select(mqmt => mqmt.SiteUserId).Distinct().ToList();
            var dbUsers = _context.SiteUsers.Where(t => currentUserIds.Contains(t.Id)).ToLookup(t => t.Id);

            var hasNewUser = false;
            foreach (var currentUserId in currentUserIds)
            {
                if (dbUsers.Contains(currentUserId))
                    continue;

                _context.SiteUsers.Add(new DbSiteUser { Id = currentUserId });
                hasNewUser = true;
            }

            if (hasNewUser)
                RecurringJob.Trigger(UserDisplayNameService.SYNC_USERS_NO_NAME);

            _context.SaveChanges();
            return null;
        }

        [HttpPost("ManuallyProcessQuestions")]
        public async Task ManuallyProcessQuestions([FromBody] ManuallyProcessQuestionsRequest request)
        {
            var isAdmin = User.HasRole(DbRole.ADMIN);
            if (!isAdmin)
                throw new HttpStatusException(HttpStatusCode.Unauthorized);

            var follows = _context.BurnakiFollows.Where(bf => bf.BurnakiId == request.FollowingId && bf.RoomId == request.RoomId).ToList();
            if (follows.Count == 0)
            {
                _logger.LogWarning($"Could not find any follows for following {request.FollowingId} in room {request.RoomId}");
                return;
            }

            if (follows.Count > 1)
            {
                _logger.LogWarning($"Found multiple follows ({follows.Count}) for following {request.FollowingId} in room {request.RoomId}");
                return;
            }

            var follow = follows.First();

            _logger.LogInformation($"Starting manual process for {string.Join(", ", request.QuestionIds)}");

            await _burnProcessingService.ProcessQuestionIds(
                request.QuestionIds,
                follow.Tag,
                null,
                true
            );

            _logger.LogInformation($"Finished manual processing for {request.QuestionIds.Count} questions");
        }

        [HttpPost("Backup")]
        public string BackupGetToken()
        {
            var isAdmin = User.HasRole(DbRole.ADMIN);
            if (!isAdmin)
                throw new HttpStatusException(HttpStatusCode.Unauthorized);

            var signingKey = GetSigningKey();
            var token = AuthenticationController.CreateJwtToken(new[] {new Claim("BackupToken", "true")}, signingKey, DateTime.UtcNow.AddMinutes(1));
            return token;
        }

        [HttpGet("Backup")]
        public FileResult Backup(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new HttpStatusException(HttpStatusCode.Unauthorized);

            var symmetricKey = GetSigningKey();
            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
            };

            var ident = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            if (!ident.Claims.Any(t => t.Type == "BackupToken" && t.Value == "true"))
                throw new HttpStatusException(HttpStatusCode.Unauthorized);
            
            var pgDumpPath = _configuration["PGDumpPath"];
            var pgDumpUsername = _configuration["PGDumpUsername"];
            var pgDumpPassword = _configuration["PGDumpPassword"];

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pgDumpPath,
                    Environment = {
                    {
                        "PGPASSWORD", pgDumpPassword
                    }},
                    Arguments = $"-U {pgDumpUsername} -h localhost rodgort",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();

            return File(process.StandardOutput.BaseStream, "text/plain", $"Rodgort - {DateTime.UtcNow:yyyy.MM.dd}.bak");
        }

        private byte[] GetSigningKey()
        {
            var signingKey = Convert.FromBase64String(_configuration["JwtSigningKey"]);
            return signingKey;
        }

        public class ResolveUnresolvedDeletionRequest
        {
            public int UnknownDeletionId { get; set; }
            public int UserId { get; set; }
            public int ActionTypeId { get; set; }
            public string Tag { get; set; }
            public DateTime DateTime { get; set; }
        }

        public class ManuallyProcessQuestionsRequest
        {
            public int RoomId { get; set; }
            public int FollowingId { get; set; }
            public List<int> QuestionIds { get; set; }
        }
    }
}
