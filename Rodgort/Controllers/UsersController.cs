using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Data.Views;
using Rodgort.Services;
using Rodgort.Utilities;
using Rodgort.Utilities.Paging;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RodgortContext _context;
        private readonly DateService _dateService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(RodgortContext context, DateService dateService, ILogger<UsersController> logger)
        {
            _context = context;
            _dateService = dateService;
            _logger = logger;
        }

        [HttpGet("actions")]
        [Authorize]
        public object Actions(int userId, string tag, int? actionTypeId, int pageNumber)
        {
            if (!User.HasRole(DbRole.TRUSTED))
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            const int pageSize = 50;

            var result = _context.Database.GetDbConnection()
                .Query(@"
select 
ua.post_id as PostId,
string_agg(ua.tag, ', ') as Tags,
uat.name as Type,
ua.time as Time
from 
user_actions ua
inner join user_action_types uat ON ua.user_action_type_id = uat.id
inner join meta_question_tags on meta_question_tags.tag_name = ua.tag
inner join meta_questions ON meta_questions.id = meta_question_tags.meta_question_id
where @userId = ua.site_user_id
and (@tag is null or @tag = ua.tag)
and (@actionTypeId is null or @actionTypeId = ua.user_action_type_id)
and meta_questions.burn_started is not null
and ua.time > meta_questions.burn_started
group by
ua.post_id, ua.user_action_type_id, uat.name, ua.time
order by ua.time desc
limit @pageSize
offset @offset
", new
                {
                    userId,
                    tag,
                    actionTypeId,
                    offset = (pageNumber - 1) * pageSize,
                    pageSize
                });

            var total = _context.Database.GetDbConnection()
                .QuerySingle<int>(@"
select count(*) 
from 
(
	select 1 from 
	user_actions ua
    inner join meta_question_tags on meta_question_tags.tag_name = ua.tag
    inner join meta_questions ON meta_questions.id = meta_question_tags.meta_question_id
	where @userId = ua.site_user_id
	and (@tag is null or @tag = ua.tag)
	and (@actionTypeId is null or @actionTypeId = ua.user_action_type_id)
    and meta_questions.burn_started is not null
    and ua.time > meta_questions.burn_started
	group by
	ua.post_id, ua.user_action_type_id, ua.time
) innerQuery", new
                {
                    userId,
                    tag,
                    actionTypeId
                });

            return new {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(1.0 * total / pageSize),
                Data = result
            };
        }
        
        [HttpGet("all")]
        public object GetAll(string userName, string sortBy, int pageNumber, int pageSize)
        { 
            IQueryable<DbUserStatisticsView> query = _context.UserStatisticsView;
            if (!string.IsNullOrWhiteSpace(userName))
                query = query.Where(u => u.DisplayName.Contains(userName));

            var constructedQuery = query.Select(q => new
            {
                q.UserId,
                q.DisplayName,
                q.IsModerator,
                q.NumBurnActions,
                q.TriagedTags,
                q.TriagedQuestions
            });

            var orderedQuery =
                sortBy == "name" ? constructedQuery.OrderBy(q => q.DisplayName)
                : sortBy == "triagedTags" ? constructedQuery.OrderByDescending(q => q.TriagedTags)
                : sortBy == "triagedQuestions" ? constructedQuery.OrderByDescending(q => q.TriagedQuestions)
                : constructedQuery.OrderByDescending(q => q.NumBurnActions);

            return orderedQuery.Page(pageNumber, pageSize);
        }
        
        [HttpGet]
        public object Get(int userId)
        {
            var isRodgortAdmin = User.HasRole(DbRole.ADMIN);

            return _context.SiteUsers
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    UserId = u.Id,
                    UserName = u.DisplayName,
                    u.IsModerator,
                    NumBurninations =
                        _context.MetaQuestions.Count(
                            mq => mq.BurnStarted.HasValue
                                  && mq.MetaQuestionTags
                                      .Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED)
                                      .Any(t => u.UserActions.Where(ua => ua.Time > mq.BurnStarted.Value).Select(ua => ua.Tag).Contains(t.TagName))
                        ),
                    Burns = _context.MetaQuestions.Where(
                        mq => mq.BurnStarted.HasValue
                              && mq.MetaQuestionTags
                                  .Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED)
                                  .Any(t => u.UserActions.Where(ua => ua.Time > mq.BurnStarted.Value).Select(ua => ua.Tag).Contains(t.TagName))
                        )
                        .Select(mq => new
                        {
                            MetaQuestionId = mq.Id,
                            MetaQuestionTitle = mq.Title,
                            StartDate = mq.BurnStarted,
                            NumActions = u.UserActions.Where(ua => 
                                mq.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED)
                                    .Select(mqt => mqt.TagName)
                                    .Contains(ua.Tag)
                            ).Select(ua => ua.PostId).Distinct().Count()
                        })
                        .OrderByDescending(mq => mq.StartDate)
                        .ToList(),
                    TriageTags = u.TagTrackingStatusAudits.Count(a => a.NewTrackingStatusId != DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL),
                    TriageQuestions = u.TagTrackingStatusAudits
                        .Where(a => a.NewTrackingStatusId != DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL)
                        .Select(audit => audit.MetaQuestionId).Distinct().Count(),
                    Roles = u.Roles.Where(r => r.Enabled).Select(r => new
                    {
                        r.RoleId,
                        r.Role.Name,
                        AddedBy = new
                        {
                            UserId = r.AddedByUserId,
                            UserName = r.AddedByUser.DisplayName,
                            r.AddedByUser.IsModerator
                        },
                        AddedByIsModerator = r.AddedByUser.IsModerator,
                        r.DateAdded
                    }).ToList(),

                    AvailableRoles = isRodgortAdmin 
                            ? _context.Roles.Where(r => !u.Roles.Where(rr => rr.Enabled).Select(rr => rr.RoleId).Contains(r.Id))
                                .Select(r => new
                                {
                                    r.Id,
                                    r.Name
                                }).ToList() 
                            : null
                })
                .FirstOrDefault();
        }

        [HttpGet("me")]
        [Authorize]
        public object Me()
        {
            return Get(User.UserId());
        }

        [Authorize]
        [HttpPost("AddRole")]
        public void AddRole([FromBody] ChangeRoleRequest request)
        {
            if (!User.HasRole(DbRole.ADMIN))
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var existingUserRole = _context.SiteUserRoles.FirstOrDefault(sur => sur.RoleId == request.RoleId && sur.UserId == request.UserId);
            if (existingUserRole != null && existingUserRole.Enabled)
                return;

            var existingRole = _context.Roles.FirstOrDefault(r => r.Id == request.RoleId);
            if (existingRole == null)
                throw new HttpStatusException(HttpStatusCode.BadRequest);

            var existingUser = _context.SiteUsers.FirstOrDefault(u => u.Id == request.UserId);
            if (existingUser == null)
                throw new HttpStatusException(HttpStatusCode.BadRequest);

            if (existingUserRole == null)
            {
                _logger.LogWarning($"{User.FindFirst(ClaimTypes.Name).Value} ({User.UserId()}) granted role '{existingRole.Name}' to {existingUser.DisplayName} ({existingUser.Id})");
                _context.SiteUserRoles.Add(new DbSiteUserRole
                {
                    AddedByUserId = User.UserId(),
                    UserId = request.UserId,
                    RoleId = request.RoleId,
                    Enabled = true,
                    DateAdded = _dateService.UtcNow
                });
            }
            else
            {
                _logger.LogWarning($"{User.FindFirst(ClaimTypes.Name).Value} ({User.UserId()}) re-activated role '{existingRole.Name}' for {existingUser.DisplayName} ({existingUser.Id})");
                existingUserRole.Enabled = true;
                existingUserRole.AddedByUserId = User.UserId();
                existingUserRole.DateAdded = _dateService.UtcNow;
            }

            _context.SiteUserRoleAudits.Add(new DbSiteUserRoleAudit
            {
                Added = true,
                ChangedByUserId = User.UserId(),
                DateChanged = _dateService.UtcNow,
                RoleId = request.RoleId,
                UserId = request.UserId
            });

            _context.SaveChanges();
        }

        [Authorize]
        [HttpPost("RemoveRole")]
        public void RemoveRole([FromBody] ChangeRoleRequest request)
        {
            if (!User.HasRole(DbRole.ADMIN))
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var existingUserRole = _context.SiteUserRoles.FirstOrDefault(sur => sur.RoleId == request.RoleId && sur.UserId == request.UserId);
            if (existingUserRole == null || !existingUserRole.Enabled)
                return;

            var existingRole = _context.Roles.FirstOrDefault(r => r.Id == request.RoleId);
            if (existingRole == null)
                throw new HttpStatusException(HttpStatusCode.BadRequest);

            var existingUser = _context.SiteUsers.FirstOrDefault(u => u.Id == request.UserId);
            if (existingUser == null)
                throw new HttpStatusException(HttpStatusCode.BadRequest);

            _logger.LogWarning($"{User.FindFirst(ClaimTypes.Name).Value} ({User.UserId()}) disabled role '{existingRole.Name}' for {existingUser.DisplayName} ({existingUser.Id})");
            existingUserRole.Enabled = false;
            
            _context.SiteUserRoleAudits.Add(new DbSiteUserRoleAudit
            {
                Added = false,
                ChangedByUserId = User.UserId(),
                DateChanged = _dateService.UtcNow,
                RoleId = request.RoleId,
                UserId = request.UserId
            });
            _context.SaveChanges();
        }

        public class ChangeRoleRequest
        {
            public int UserId { get; set; }
            public int RoleId { get; set; }
        }
    }
}