using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Services;
using Rodgort.Utilities;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RodgortContext _context;
        private readonly DateService _dateService;

        public UsersController(RodgortContext context, DateService dateService)
        {
            _context = context;
            _dateService = dateService;
        }

        [HttpGet]
        public object Get(int userId)
        {
            var isRodgortAdmin = User.HasClaim(DbRole.RODGORT_ADMIN);

            return _context.SiteUsers
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    UserId = u.Id,
                    u.DisplayName,
                    u.IsModerator,
                    NumBurninations =
                        _context.MetaQuestions.Count(
                            mq => mq.BurnStarted.HasValue
                                  && mq.MetaQuestionTags
                                      .Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED)
                                      .Any(t => u.UserActions.Select(ua => ua.Tag).Contains(t.TagName))
                        ),
                    Burns = _context.MetaQuestions.Where(
                        mq => mq.BurnStarted.HasValue
                              && mq.MetaQuestionTags
                                  .Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED)
                                  .Any(t => u.UserActions.Select(ua => ua.Tag).Contains(t.TagName))
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
                    TriageTags = u.TagTrackingStatusAudits.Count,
                    TriageQuestions = u.TagTrackingStatusAudits.Select(audit => audit.MetaQuestionId).Distinct().Count(),
                    Roles = u.Roles.Where(r => r.Enabled).Select(r => new
                    {
                        Name = r.RoleName,
                        AddedById = r.AddedByUserId,
                        AddedBy = r.AddedByUser.DisplayName,
                        AddedByIsModerator = r.AddedByUser.IsModerator,
                        r.DateAdded
                    }).ToList(),

                    AvailableRoles = isRodgortAdmin 
                            ? _context.Roles.Where(r => !u.Roles.Where(rr => rr.Enabled).Select(rr => rr.RoleName).Contains(r.Name))
                                .Select(r => new
                                {
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
            if (!User.HasClaim(DbRole.RODGORT_ADMIN))
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var existingRole = _context.SiteUserRoles.FirstOrDefault(sur => sur.RoleName == request.RoleName && sur.UserId == request.UserId);
            if (existingRole != null && existingRole.Enabled)
                return;

            var roleExists = _context.Roles.FirstOrDefault(r => r.Name == request.RoleName);
            if (roleExists == null)
                throw new HttpStatusException(HttpStatusCode.BadRequest);

            var userExists = _context.SiteUsers.FirstOrDefault(u => u.Id == request.UserId);
            if (userExists == null)
                throw new HttpStatusException(HttpStatusCode.BadRequest);

            if (existingRole == null)
            {
                _context.SiteUserRoles.Add(new DbSiteUserRole
                {
                    AddedByUserId = User.UserId(),
                    UserId = request.UserId,
                    RoleName = request.RoleName,
                    Enabled = true,
                    DateAdded = _dateService.UtcNow
                });
            }
            else
            {
                existingRole.Enabled = true;
                existingRole.AddedByUserId = User.UserId();
                existingRole.DateAdded = _dateService.UtcNow;
            }

            _context.SiteUserRoleAudits.Add(new DbSiteUserRoleAudit
            {
                Added = true,
                ChangedByUserId = User.UserId(),
                DateChanged = _dateService.UtcNow,
                RoleName = request.RoleName,
                UserId = request.UserId
            });

            _context.SaveChanges();
        }

        [Authorize]
        [HttpPost("RemoveRole")]
        public void RemoveRole([FromBody] ChangeRoleRequest request)
        {
            if (!User.HasClaim(DbRole.RODGORT_ADMIN))
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var existingRole = _context.SiteUserRoles.FirstOrDefault(sur => sur.RoleName == request.RoleName && sur.UserId == request.UserId);
            if (existingRole == null || !existingRole.Enabled)
                return;

            var roleExists = _context.Roles.FirstOrDefault(r => r.Name == request.RoleName);
            if (roleExists == null)
                throw new HttpStatusException(HttpStatusCode.BadRequest);

            var userExists = _context.SiteUsers.FirstOrDefault(u => u.Id == request.UserId);
            if (userExists == null)
                throw new HttpStatusException(HttpStatusCode.BadRequest);

            existingRole.Enabled = false;

            _context.SiteUserRoleAudits.Add(new DbSiteUserRoleAudit
            {
                Added = false,
                ChangedByUserId = User.UserId(),
                DateChanged = _dateService.UtcNow,
                RoleName = request.RoleName,
                UserId = request.UserId
            });
            _context.SaveChanges();
        }

        public class ChangeRoleRequest
        {
            public int UserId { get; set; }
            public string RoleName { get; set; }
        }
    }
}