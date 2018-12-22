using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Services;

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

            var burninationRequestsWithApproved = _context.MetaQuestions.Count(mq => mq.MetaQuestionTags.Any(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED));
            var burninationRequestsRequireApproval = _context.MetaQuestions.Count(mq => mq.MetaQuestionTags.Any(mqt => mqt.StatusId == DbMetaQuestionTagStatus.PENDING));
            
            var declinedRequests = _context.MetaQuestions.Count(mq => mq.MetaQuestionMetaTags.Any(mtqm => mtqm.TagName == DbMetaTag.STATUS_DECLINED));
            var completedRequests = _context.MetaQuestions.Count(mq => mq.MetaQuestionMetaTags.Any(mtqm => mtqm.TagName == DbMetaTag.STATUS_COMPLETED));

            var completedRequestsWithQuestions = _context.MetaQuestions.Count(mq => 
                mq.MetaQuestionMetaTags.Any(mtqm => mtqm.TagName == DbMetaTag.STATUS_COMPLETED)
                && mq.MetaQuestionTags.Where(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED).Any(mqt => mqt.Tag.NumberOfQuestions > 0)
            );

            var statusTags = DbMetaTag.StatusFlags;
            var noStatusNoQuestions = _context.MetaQuestions.Count(mq => 
                !mq.MetaQuestionMetaTags.Any(mqa => statusTags.Contains(mqa.TagName))
                && !mq.ClosedDate.HasValue
                && mq.MetaQuestionTags.Any(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED)
                && mq.MetaQuestionTags.Where(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED).All(mqt => mqt.Tag.NumberOfQuestions <= 0)
            );

            var filteredTags = _context.Tags.Where(t => t.MetaQuestionTags.Any(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED));
            var tagCount = filteredTags.Count();
            var tagsNoQuestions = filteredTags.Count(t => t.NumberOfQuestions == 0);
            var synonomisedTags = filteredTags.Count(t => t.SynonymOf != null);

            var tagsQuestionsOnCompleted = filteredTags.Count(t =>
                t.NumberOfQuestions > 0
                && t.MetaQuestionTags.Any(mqt =>
                    mqt.StatusId == DbMetaQuestionTagStatus.APPROVED
                    && mqt.MetaQuestion.MetaQuestionMetaTags.Any(mqma => mqma.TagName == DbMetaTag.STATUS_COMPLETED))
            );

            return new
            {
                Requests = new
                {
                    Total = burninationRequests,

                    WithApprovedTags = burninationRequestsWithApproved,
                    RequireApproval = burninationRequestsRequireApproval,
                    
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
                }
            };
        }

        [HttpGet("Leaderboard/Current")]
        public object CurrentLeaderboard()
        {
            var burnsData = _context.MetaQuestions.Where(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_PLANNED))
                .Select(mq => new
                {
                    mq.FeaturedStarted,
                    mq.FeaturedEnded,
                    mq.BurnStarted,
                    mq.BurnEnded,
                    BurningTags = mq.MetaQuestionTags.Where(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED)
                        .Select(mqt =>
                            new
                            {
                                Tag = mqt.TagName,
                                mqt.Tag.NumberOfQuestions,
                                QuestionCountOverTime = mqt.Tag.Statistics.Select(s => new { s.DateTime, s.QuestionCount }).OrderBy(s => s.DateTime).ToList(),
                                Actions = _context.UserActions.Where(ua => ua.Tag == mqt.TagName).Select(ua => new
                                {
                                    ua.PostId,
                                    User = ua.SiteUser.DisplayName ?? ua.SiteUserId.ToString(),
                                    ua.Time,
                                    Type = ua.UserActionType.Name
                                }).ToList()
                            }).ToList()
                }).ToList();

            var firstTime = DateTime.MinValue;
            var res = new
            {
                Burns = burnsData.Select(b => new
                {
                    Tags = b.BurningTags.Select(bt => new
                    {
                        bt.Tag,
                        bt.NumberOfQuestions,
                        bt.QuestionCountOverTime,
                        Overtime = bt.Actions.Where(a => a.Time > firstTime)
                            .GroupBy(a => a.User)
                            .Select(g => new
                            {
                                User = g.Key,
                                Times = g.GroupBy(gg => new { Time = gg.Time.Date.AddHours(gg.Time.Hour) })
                                    .Select(gg => new
                                    {
                                        Date = gg.Key.Time,
                                        Total = g.Where(ggg => ggg.Time.Date.AddHours(ggg.Time.Hour) <= gg.Key.Time).Select(ggg => ggg.PostId).Distinct().Count()
                                    })
                                    .OrderBy(gg => gg.Date)
                            })
                            .OrderByDescending(g => g.Times.Count())
                            .Take(10)
                        ,
                        UserTotals = bt.Actions.Where(a => a.Time > firstTime).GroupBy(g => new {g.Type, g.User}).Select(g => new
                        {
                            g.Key.User,
                            g.Key.Type,
                            Total = g.Select(gg => gg.PostId).Distinct().Count()
                        }), 
                        Totals = bt.Actions.Where(a => a.Time > firstTime).GroupBy(g => g.Type).Select(g => new
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