using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                            || mqt.TagName == DbMetaTag.STATUS_PLANNED
                            || mqt.TagName == DbMetaTag.STATUS_FEATURED)
                    );
                else
                    query = query.Where(mq => mq.MetaQuestionMetaTags.Any(mqt => mqt.TagName == status));
            }

            var modTags = new[]
            {
                DbMetaTag.STATUS_FEATURED, DbMetaTag.STATUS_COMPLETED, DbMetaTag.STATUS_DECLINED,
                DbMetaTag.STATUS_PLANNED
            };

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
                    }),
                    MetaStatusTags = mq.MetaQuestionMetaTags.Where(mqmt => modTags.Contains(mqmt.TagName)).Select(mqt => new
                    {
                        mqt.TagName
                    }),
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
                    matchingQuestionMetaTag.StatusId = newStatusId;
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


        [HttpPost("SetTagRequestType")]
        public void SetTagRequestType([FromBody] SetTagRequestTypeRequest request)
        {
            var matchingQuestionMetaTag = _context.MetaQuestionTags.FirstOrDefault(mqt => mqt.MetaQuestionId == request.MetaQuestionId && mqt.TagName == request.TagName);
            if (matchingQuestionMetaTag != null)
            {
                var matchingRequestType = _context.RequestTypes.FirstOrDefault(rt => rt.Name == request.RequestType);
                if (matchingRequestType == null)
                    throw new ArgumentException($"Request type {request.RequestType} invalid.");

                var newRequestTypeId = matchingRequestType.Id;
                
                if (matchingQuestionMetaTag.RequestTypeId != newRequestTypeId)
                {
                    matchingQuestionMetaTag.RequestTypeId = newRequestTypeId;
                    _context.SaveChanges();
                }
            }
        }

        public class SetTagRequestTypeRequest
        {
            public int MetaQuestionId { get; set; }
            public string TagName { get; set; }
            public string RequestType { get; set; }
        }


        [HttpPost("AddTag")]
        public void AddTag([FromBody] AddTagRequest request)
        {
            var matchingQuestionMetaTag = _context.MetaQuestions
                .Include(mq => mq.MetaQuestionTags)
                .FirstOrDefault(mqt => mqt.Id == request.MetaQuestionId);

            if (matchingQuestionMetaTag != null)
            {
                var matchingRequestType = _context.RequestTypes.FirstOrDefault(rt => rt.Name == request.RequestType);
                if (matchingRequestType == null)
                    throw new ArgumentException($"Request type {request.RequestType} invalid.");

                var alreadyExistingTag = matchingQuestionMetaTag.MetaQuestionTags.FirstOrDefault(mqt => mqt.TagName == request.TagName);
                if (alreadyExistingTag != null)
                {
                    alreadyExistingTag.StatusId = DbMetaQuestionTagStatus.APPROVED;
                }
                else
                {
                    var tagExists = _context.Tags.Any(mt => mt.Name == request.TagName);
                    if (!tagExists)
                        _context.Tags.Add(new DbTag { Name = request.TagName });

                    _context.MetaQuestionTags.Add(new DbMetaQuestionTag
                    {
                        MetaQuestionId = request.MetaQuestionId,
                        RequestTypeId = matchingRequestType.Id,
                        StatusId = DbMetaQuestionTagStatus.APPROVED,
                        TagName = request.TagName
                    });
                }

                _context.SaveChanges();
            }
        }

        public class AddTagRequest
        {
            public int MetaQuestionId { get; set; }
            public string TagName { get; set; }
            public string RequestType { get; set; }
        }
    }
}