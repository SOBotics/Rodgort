using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Services;
using Rodgort.Utilities;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class StatisticsController : Controller
    {
        private readonly RodgortContext _context;
        private readonly DateService _dateService;

        public StatisticsController(RodgortContext context, DateService dateService)
        {
            _context = context;
            _dateService = dateService;
        }

        [HttpGet]
        public object Get()
        {
            var burninationRequests = _context.MetaQuestions.Count();

            var currentBurns = _context.MetaQuestions.Count(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_PLANNED));
            var proposedBurns = _context.MetaQuestions.Count(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_FEATURED));

            var burninationRequestsWithTracked = _context.MetaQuestions.Count(mq => mq.MetaQuestionTags.Any(mqt => 
                mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE));
            var burninationRequestsRequireTrackingApproval = _context.MetaQuestions.Count(mq => mq.MetaQuestionTags.Any(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL));
            
            var declinedRequests = _context.MetaQuestions.Count(mq => mq.MetaQuestionMetaTags.Any(mtqm => mtqm.TagName == DbMetaTag.STATUS_DECLINED));
            var completedRequests = _context.MetaQuestions.Count(mq => mq.MetaQuestionMetaTags.Any(mtqm => mtqm.TagName == DbMetaTag.STATUS_COMPLETED));

            var unknownDeletions = _context.UserActions.Count(ua => ua.UserActionTypeId == DbUserActionType.UNKNOWN_DELETION);

            var completedRequestsWithQuestions = _context.MetaQuestions.Count(mq => 
                mq.MetaQuestionMetaTags.Any(mtqm => mtqm.TagName == DbMetaTag.STATUS_COMPLETED)
                && mq.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED).Any(mqt => mqt.Tag.NumberOfQuestions > 0)
            );

            var statusTags = DbMetaTag.StatusFlags;
            var noStatusNoQuestions = _context.MetaQuestions.Count(mq => 
                !mq.MetaQuestionMetaTags.Any(mqa => statusTags.Contains(mqa.TagName))
                && !mq.ClosedDate.HasValue
                && mq.MetaQuestionTags.Any(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                                                  || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE)
                && mq.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                                                    || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE).All(mqt => mqt.Tag.NumberOfQuestions <= 0)
            );

            var filteredTags = _context.Tags.Where(t => t.MetaQuestionTags.Any(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                                                                                      || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE));
            var tagCount = filteredTags.Count();
            var tagsNoQuestions = filteredTags.Count(t => t.NumberOfQuestions == 0);
            var synonomisedTags = filteredTags.Count(t => t.SynonymOf != null);

            var tagsQuestionsOnCompleted = filteredTags.Count(t =>
                t.NumberOfQuestions > 0
                && t.MetaQuestionTags.Any(mqt =>
                    (mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                    || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE)
                    && mqt.MetaQuestion.MetaQuestionMetaTags.Any(mqma => mqma.TagName == DbMetaTag.STATUS_COMPLETED))
            );

            return new
            {
                Requests = new
                {
                    Total = burninationRequests,

                    CurrentBurns = currentBurns,
                    ProposedBurns = proposedBurns,

                    WithTrackedTags = burninationRequestsWithTracked,
                    RequireTrackingApproval = burninationRequestsRequireTrackingApproval,
                    
                    Declined = declinedRequests,
                    Completed = completedRequests,

                    CompletedWithQuestionsLeft = completedRequestsWithQuestions,
                    NoStatusButCompleted = noStatusNoQuestions
                }, 
                Tags = new
                {
                    Total = tagCount,
                    noQuestions = tagsNoQuestions,
                    synonymised = synonomisedTags,
                    hasQuestionsAndAttachedToCompletedRequest = tagsQuestionsOnCompleted
                },
                Admin = new
                {
                    UnknownDeletions = unknownDeletions
                }
            };
        }

        [HttpGet("Leaderboard/Current")]
        public object CurrentLeaderboard()
        {
            return GenerateBurnsData(_context.MetaQuestions.Where(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_PLANNED)));
        }

        [HttpGet("Leaderboard")]
        public object LeaderboardForId(int metaQuestionId)
        {
            return GenerateBurnsData(_context.MetaQuestions.Where(mq => mq.Id == metaQuestionId));
        }

        private object GenerateBurnsData(IQueryable<DbMetaQuestion> query)
        {
            var isRoomOwner = User.HasClaim(DbRole.TROGDOR_ROOM_OWNER);

            var burnsData = query
                .Select(mq => new
                {
                    mq.FeaturedStarted,
                    mq.FeaturedEnded,
                    mq.BurnStarted,
                    mq.BurnEnded,
                    mq.Title,
                    mq.Link,
                    BurningTags = mq.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE)
                        .Select(mqt =>
                            new
                            {
                                Tag = mqt.TagName,
                                mqt.Tag.NumberOfQuestions,
                                QuestionCountOverTime = mqt.Tag.Statistics.Where(s => s.DateTime > (mq.FeaturedStarted ?? mq.FeaturedEnded ?? mq.BurnStarted ?? mq.BurnEnded))
                                    .Select(s => new { s.DateTime, s.QuestionCount }).OrderBy(s => s.DateTime).ToList(),
                                Actions = _context.UserActions
                                    .Where(a => a.UserActionTypeId != DbUserActionType.UNKNOWN_DELETION)
                                    .Where(ua => ua.Tag == mqt.TagName).Select(ua => new
                                    {
                                        ua.PostId,
                                        UserId = ua.SiteUserId,
                                        User = ua.SiteUser.DisplayName ?? ua.SiteUserId.ToString(),
                                        ua.Time,
                                        TypeId = ua.UserActionTypeId,
                                        Type = ua.UserActionType.Name
                                    }).ToList()
                            }).ToList()
                }).ToList();

            var res = new
            {
                Burns = burnsData.Select(b => new
                {
                    MetaQuestionTitle = b.Title,
                    MetaQuestionLink = b.Link,
                    Tags = b.BurningTags.Select(bt => new
                    {
                        bt.Tag,
                        bt.NumberOfQuestions,
                        bt.QuestionCountOverTime,

                        ClosuresOverTime = bt.Actions.GroupBy(gg => new { Time = gg.Time.Date.AddHours(gg.Time.Hour) })
                            .Select(gg => new
                            {
                                Date = gg.Key.Time,
                                Total =
                                    bt.Actions.Where(ggg => 
                                        ggg.TypeId == DbUserActionType.CLOSED 
                                        && ggg.Time.Date.AddHours(ggg.Time.Hour) <= gg.Key.Time
                                        && !bt.Actions.Any(a => a.PostId == ggg.PostId && a.TypeId == DbUserActionType.DELETED)
                                    ).Select(ggg => ggg.PostId).Distinct().Count()
                                    - bt.Actions.Where(ggg => 
                                        ggg.TypeId == DbUserActionType.REOPENED && 
                                        ggg.Time.Date.AddHours(ggg.Time.Hour) <= gg.Key.Time
                                        && !bt.Actions.Any(a => a.PostId == ggg.PostId && a.TypeId == DbUserActionType.DELETED)
                                    ).Select(ggg => ggg.PostId).Distinct().Count()
                            })
                            .Where(s => s.Date > (b.FeaturedStarted ?? b.FeaturedEnded ?? b.BurnStarted ?? b.BurnEnded))
                            .OrderBy(gg => gg.Date),

                        DeletionsOverTime = bt.Actions.GroupBy(gg => new { Time = gg.Time.Date.AddHours(gg.Time.Hour) })
                            .Select(gg => new
                            {
                                Date = gg.Key.Time,
                                Total =
                                    bt.Actions.Where(ggg => ggg.TypeId == DbUserActionType.DELETED && ggg.Time.Date.AddHours(ggg.Time.Hour) <= gg.Key.Time).Select(ggg => ggg.PostId).Distinct().Count()
                                    - bt.Actions.Where(ggg => ggg.TypeId == DbUserActionType.UNDELETED && ggg.Time.Date.AddHours(ggg.Time.Hour) <= gg.Key.Time).Select(ggg => ggg.PostId).Distinct().Count()
                            })
                            .Where(s => s.Date > (b.FeaturedStarted ?? b.FeaturedEnded ?? b.BurnStarted ?? b.BurnEnded))
                            .OrderBy(gg => gg.Date),

                        RetagsOverTime = bt.Actions.GroupBy(gg => new { Time = gg.Time.Date.AddHours(gg.Time.Hour) })
                            .Select(gg => new
                            {
                                Date = gg.Key.Time,
                                Total =
                                    bt.Actions.Where(ggg => ggg.TypeId == DbUserActionType.REMOVED_TAG && ggg.Time.Date.AddHours(ggg.Time.Hour) <= gg.Key.Time).Select(ggg => ggg.PostId).Distinct().Count()
                                    - bt.Actions.Where(ggg => ggg.TypeId == DbUserActionType.ADDED_TAG && ggg.Time.Date.AddHours(ggg.Time.Hour) <= gg.Key.Time).Select(ggg => ggg.PostId).Distinct().Count()
                            })
                            .Where(s => s.Date > (b.FeaturedStarted ?? b.FeaturedEnded ?? b.BurnStarted ?? b.BurnEnded))
                            .OrderBy(gg => gg.Date),

                        RoombasOverTime = bt.Actions.GroupBy(gg => new { Time = gg.Time.Date.AddHours(gg.Time.Hour) })
                            .Select(gg => new
                            {
                                Date = gg.Key.Time,
                                Total = bt.Actions.Where(ggg => ggg.TypeId == DbUserActionType.DELETED && ggg.UserId == -1 && ggg.Time.Date.AddHours(ggg.Time.Hour) <= gg.Key.Time).Select(ggg => ggg.PostId).Distinct().Count()
                            })
                            .Where(s => s.Date > (b.FeaturedStarted ?? b.FeaturedEnded ?? b.BurnStarted ?? b.BurnEnded))
                            .OrderBy(gg => gg.Date),

                        Overtime = bt.Actions
                            .Where(ua => ua.Time > (b.FeaturedStarted ?? b.FeaturedEnded ?? b.BurnStarted ?? b.BurnEnded))
                            .Where(ua => isRoomOwner || ua.Time > b.BurnStarted)
                            .GroupBy(a => new { a.User, a.UserId })
                            .Select(g => new
                            {
                                g.Key.User,
                                Times = g.GroupBy(gg => new { Time = gg.Time.Date.AddHours(gg.Time.Hour) })
                                    .Select(gg => new
                                    {
                                        Date = gg.Key.Time,
                                        Total = g.Where(ggg => ggg.Time.Date.AddHours(ggg.Time.Hour) <= gg.Key.Time).Select(ggg => ggg.PostId).Distinct().Count()
                                    })
                                    .OrderBy(gg => gg.Date)
                            })
                            .OrderByDescending(g => g.Times.Max(tt => tt.Total))
                            .Take(10)
                        ,
                        UserTotals = bt.Actions
                            .Where(ua => ua.Time > (b.FeaturedStarted ?? b.FeaturedEnded ?? b.BurnStarted ?? b.BurnEnded))
                            .Where(ua => isRoomOwner || ua.Time > b.BurnStarted)
                            .GroupBy(g => new { g.Type, g.User, g.UserId }).Select(g => new
                        {
                            UserName = g.Key.User,
                            g.Key.UserId,
                            g.Key.Type,
                            Total = g.Select(gg => gg.PostId).Distinct().Count()
                        }),
                        UserGrandTotals = bt.Actions
                            .Where(ua => ua.Time > (b.FeaturedStarted ?? b.FeaturedEnded ?? b.BurnStarted ?? b.BurnEnded))
                            .Where(ua => isRoomOwner || ua.Time > b.BurnStarted)
                            .GroupBy(g => new { g.User, g.UserId }).Select(g => new
                        {
                            UserName = g.Key.User,
                            g.Key.UserId,
                            Total = g.Select(gg => gg.PostId).Distinct().Count()
                        }),
                        Totals = bt.Actions
                            .Where(ua => ua.Time > (b.FeaturedStarted ?? b.FeaturedEnded ?? b.BurnStarted ?? b.BurnEnded))
                            .GroupBy(g => g.Type).Select(g => new
                        {
                            Type = g.Key,
                            Total = g.Select(gg => gg.PostId).Distinct().Count()
                        })
                    }),

                    b.FeaturedStarted,
                    b.FeaturedEnded,
                    b.BurnStarted,
                    b.BurnEnded
                })
            };
            return res;
        }
    }
}