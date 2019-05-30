using System;
using System.Linq;
using System.Net;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public UsersController(RodgortContext context, DateService dateService)
        {
            _context = context;
            _dateService = dateService;
        }

        [HttpGet("actions")]
        [Authorize]
        public object Actions(int userId, string tag, int? actionTypeId, int pageNumber)
        {
            if (!User.HasRole(DbRole.TRUSTED))
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            const int pageSize = 50;

            var result = _context.Database.GetDbConnection()
                .Query<ActionsResult>(@"
select 
ua.post_id as PostId,
string_agg(ua.tag, ', ') as Tags,
uat.name as Type,
ua.time as Time
from 
user_actions ua
inner join user_action_types uat ON ua.user_action_type_id = uat.id
where @userId = ua.site_user_id
and (@tag is null or @tag = ua.tag)
and (@actionTypeId is null or @actionTypeId = ua.user_action_type_id)
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
	where @userId = ua.site_user_id
	and (@tag is null or @tag = ua.tag)
	and (@actionTypeId is null or @actionTypeId = ua.user_action_type_id)
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

        private class ActionsResult
        {
            public int PostId { get; set; }
            public string Tags { get; set; }
            public string Type { get; set; }
            public DateTime Time { get; set; }
        }

        [HttpGet("all")]
        public object GetAll(string userName, int pageNumber, int pageSize)
        { 
            IQueryable<DbUserStatisticsView> query = _context.UserStatisticsView;
            if (!string.IsNullOrWhiteSpace(userName))
                query = query.Where(u => u.DisplayName.Contains(userName));

            return query.OrderByDescending(su => su.NumBurnActions).Page(pageNumber, pageSize);
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
                        r.RoleId,
                        r.Role.Name,
                        AddedBy = new
                        {
                            UserId = r.AddedByUserId,
                            UserName = r.AddedByUser.DisplayName,
                            IsModerator = r.AddedByUser.IsModerator
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

            var existingRole = _context.SiteUserRoles.FirstOrDefault(sur => sur.RoleId == request.RoleId && sur.UserId == request.UserId);
            if (existingRole != null && existingRole.Enabled)
                return;

            var roleExists = _context.Roles.FirstOrDefault(r => r.Id == request.RoleId);
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
                    RoleId = request.RoleId,
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

            var existingRole = _context.SiteUserRoles.FirstOrDefault(sur => sur.RoleId == request.RoleId && sur.UserId == request.UserId);
            if (existingRole == null || !existingRole.Enabled)
                return;

            var roleExists = _context.Roles.FirstOrDefault(r => r.Id == request.RoleId);
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