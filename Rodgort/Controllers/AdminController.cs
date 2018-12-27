using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Services;
using Rodgort.Utilities;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly RodgortContext _context;

        public AdminController(RodgortContext context)
        {
            _context = context;
        }

        [HttpGet("UnresolvedDeletions")]
        public object GetUnresolvedDeletions()
        {
            var isAdmin = User.HasClaim(DbRole.RODGORT_ADMIN);
            if (!isAdmin)
                throw new HttpStatusException(HttpStatusCode.Unauthorized);

            return _context.UserActions
                .Where(ua => ua.SiteUserId == -1)
                .Where(ua => ua.UserActionTypeId == DbUserActionType.UNKNOWN_DELETION)
                .Select(ua => new
                {
                    ActionId = ua.Id,
                    ua.PostId
                })
                .ToList();
        }

        [HttpPost("ResolveUnresolvedDeletion")]
        public object ResolveUnresolvedDeletion([FromBody] List<ResolveUnresolvedDeletionRequest> requestItems)
        {
            var isAdmin = User.HasClaim(DbRole.RODGORT_ADMIN);
            if (!isAdmin)
                throw new HttpStatusException(HttpStatusCode.Unauthorized);

            foreach (var requestItemsPerAction in requestItems.GroupBy(g => g.ActionId))
            {
                var matchedUnknown = _context.UserActions.FirstOrDefault(ua => ua.Id == requestItemsPerAction.Key);
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
                        UserActionTypeId = requestItem.ActionTypeId
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

                _context.UserActions.Remove(matchedUnknown);
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

        public class ResolveUnresolvedDeletionRequest
        {
            public int ActionId { get; set; }
            public int UserId { get; set; }
            public int ActionTypeId { get; set; }
            public string Tag { get; set; }
            public DateTime DateTime { get; set; }
        }
    }
}
