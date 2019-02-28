﻿using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
using Rodgort.Data.Tables;
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
        public object Actions(int userId, string tag, int actionTypeId, int pageNumber)
        {
            if (!User.HasRole(DbRole.TRUSTED))
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var query = _context.UserActions.Where(u => u.SiteUserId == userId);
            if (!string.IsNullOrWhiteSpace(tag))
                query = query.Where(ua => ua.Tag == tag);
            if (actionTypeId > 0)
                query = query.Where(ua => ua.UserActionTypeId == actionTypeId);

            return query
                .Select(q => new
                {
                    q.PostId,
                    Tags = q.Tag,
                    Type = q.UserActionType.Name,
                    q.Time
                })
                .OrderByDescending(q => q.Time)
                .Page(pageNumber, 50);
        }

        [HttpGet("all")]
        public object GetAll(int pageNumber, int pageSize)
        {
            var query = _context.SiteUsers
                .Select(su => new
                {
                    su.Id,
                    su.DisplayName,
                    su.IsModerator,
                    NumBurnActions = 
                            su.UserActions.Join(
                                _context.MetaQuestionTags
                                    .Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED)
                                    .Where(mqt => mqt.MetaQuestion.BurnStarted.HasValue),
                                ua => ua.Tag, 
                                mqt => mqt.TagName, 
                                (userAction, metaQuestionTag) => new { metaQuestionTag, userAction}
                            )
                            .Select(a => a.userAction.PostId).Distinct().Count(),

                    TriageTags = su.TagTrackingStatusAudits.Count,
                    TriageQuestions = su.TagTrackingStatusAudits.Select(audit => audit.MetaQuestionId).Distinct().Count(),
                })
                .OrderByDescending(su => su.NumBurnActions);

            return query.Page(pageNumber, pageSize);
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