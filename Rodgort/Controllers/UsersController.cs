using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Utilities;
using Rodgort.Utilities.Paging;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RodgortContext _context;

        public UsersController(RodgortContext context)
        {
            _context = context;
        }

        [HttpGet]
        public object Get(int userId)
        {
            return _context.SiteUsers
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    UserId = u.Id,
                    u.DisplayName,
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
                    Roles = u.Roles.Select(r => new
                    {
                        Name = r.RoleName,
                        AddedBy = r.AddedByUser.DisplayName,
                        AddedById = r.AddedByUserId,
                        r.DateAdded
                    }).ToList()
                })
                .FirstOrDefault();
        }

        [HttpGet("me")]
        [Authorize]
        public object Me()
        {
            return Get(User.UserId());
        }
    }
}