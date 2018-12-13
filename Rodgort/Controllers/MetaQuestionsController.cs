using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Utilities.Paging;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class MetaQuestionsController : Controller
    {
        private readonly RodgortContext _context;

        public MetaQuestionsController(RodgortContext context)
        {
            _context = context;
        }

        [HttpGet]
        public object Get(
            string tag = null,
            int approvalStatus = -1,
            int type = -1,
            string status = null,
            string sortBy = null,
            int page = 1, 
            int pageSize = 30)
        {
            IQueryable<DbMetaQuestion> query = _context.MetaQuestions;
            if (!string.IsNullOrWhiteSpace(tag))
                query = query.Where(mq => mq.MetaQuestionTags.Any(mqt => mqt.TagName == tag));

            if (approvalStatus > 0)
                query = query.Where(mq => mq.MetaQuestionTags.Any(mqt => mqt.StatusId == approvalStatus));

            if (type > 0)
                query = query.Where(mq => mq.MetaQuestionTags.Any(mqt => mqt.RequestTypeId == type));

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "none")
                    query = query.Where(mq =>
                        !mq.MetaQuestionMetaTags.Any(mqt =>
                            mqt.TagName == DbMetaTag.STATUS_COMPLETED
                            || mqt.TagName == DbMetaTag.STATUS_DECLINED
                            || mqt.TagName == DbMetaTag.STATUS_PLANNED)
                    );
                else
                    query = query.Where(mq => mq.MetaQuestionMetaTags.Any(mqt => mqt.TagName == status));
            }

            var transformedQuery = query
                .Select(mq => new
                {
                    mq.Id,
                    mq.Title,
                    mq.Score,
                    MainTags = mq.MetaQuestionTags.Select(mqt => new
                    {
                        mqt.TagName,
                        Type = mqt.RequestType.Name,
                        mqt.StatusId,
                        Status = mqt.Status.Name,
                        QuestionCountOverTime = mqt.Tag.Statistics.Select(s => new {s.DateTime, s.QuestionCount})
                    }).OrderBy(mqt => mqt.StatusId),
                    NumQuestions = mq.MetaQuestionTags.Where(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED)
                        .Select(mqt => mqt.Tag.NumberOfQuestions)
                        .OrderByDescending(mqt => mqt)
                        .FirstOrDefault(),
                    ScoreOverTime = mq.Statistics.Select(s => new {s.DateTime, s.Score}),
                });

            IOrderedQueryable<object> orderedQuery;
            if (sortBy == "score")
                orderedQuery = transformedQuery.OrderByDescending(t => t.Score);
            else if (sortBy == "numQuestions")
                orderedQuery = transformedQuery.OrderByDescending(t => t.NumQuestions);
            else
                orderedQuery = transformedQuery.OrderByDescending(t => t.Id);

            var result = orderedQuery.Page(page, pageSize);
            return result;
        }

        [HttpPost("SetTagApprovalStatus")]
        public void SetTagApprovalStatus([FromBody] SetApprovalStatusRequest request)
        {
            var matchingQuestionMetaTag = _context.MetaQuestionTags.FirstOrDefault(mqt => mqt.MetaQuestionId == request.MetaQuestionId && mqt.TagName == request.TagName);
            if (matchingQuestionMetaTag != null)
            {
                var newStatusId =
                    request.Approved
                        ? DbMetaQuestionTagStatus.APPROVED
                        : DbMetaQuestionTagStatus.REJECTED;

                if (matchingQuestionMetaTag.StatusId != newStatusId)
                {
                    matchingQuestionMetaTag.StatusId = newStatusId;;
                    _context.SaveChanges();
                }
            }
        }

        public class SetApprovalStatusRequest
        {
            public int MetaQuestionId { get; set; }
            public string TagName { get; set; }
            public bool Approved { get; set; }
        }
    }
}