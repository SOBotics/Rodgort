using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
using Rodgort.Data.Tables;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class StatisticsController : Controller
    {
        private readonly RodgortContext _context;

        public StatisticsController(RodgortContext context)
        {
            _context = context;
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
                && t.MetaQuestionTags.Any(mqt => mqt.MetaQuestion.MetaQuestionMetaTags.Any(mqma => mqma.TagName == DbMetaTag.STATUS_COMPLETED))
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
    }
}