using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Services;
using Rodgort.Utilities;
using Rodgort.Utilities.Paging;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class MetaQuestionsController : Controller
    {
        private readonly RodgortContext _context;
        private readonly DateService _dateService;

        public MetaQuestionsController(RodgortContext context, DateService dateService)
        {
            _context = context;
            _dateService = dateService;
        }

        [HttpGet]
        public object Get(
            string query = null,
            string searchBy = null,
            int trackingStatusId = -1,
            string status = null,
            string requestType = null,
            string hasQuestions = null,
            string sortBy = null,
            int page = 1, 
            int pageSize = 30)
        {
            IQueryable<DbMetaQuestion> dbQuery = _context.MetaQuestions;
            if (!string.IsNullOrWhiteSpace(query))
            {
                switch (searchBy)
                {
                    case "id":
                    {
                        dbQuery = int.TryParse(query, out var id)
                            ? dbQuery.Where(mq => mq.Id == id)
                            : dbQuery.Where(mq => false);
                        break;
                    }
                    case "tag":
                        dbQuery = dbQuery.Where(mq => mq.MetaQuestionTags.Any(mqt => mqt.TagName == query));
                        break;
                    case "content":
                        dbQuery = dbQuery.Where(mq => mq.Body.Contains(query) || mq.Title.Contains(query));
                        break;
                    default:
                    {
                        dbQuery = int.TryParse(query, out var id)
                            ? dbQuery.Where(mq => mq.Id == id || mq.MetaQuestionTags.Any(mqt => mqt.TagName == query) || mq.Body.Contains(query) || mq.Title.Contains(query))
                            : dbQuery.Where(mq => mq.MetaQuestionTags.Any(mqt => mqt.TagName == query) || mq.Body.Contains(query) || mq.Title.Contains(query));
                        break;
                    }
                }
            }

            if (trackingStatusId != -1)
            {
                dbQuery = trackingStatusId == -10
                    ? dbQuery.Where(mq => mq.MetaQuestionTags.Any(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE))
                    : dbQuery.Where(mq => mq.MetaQuestionTags.Any(mqt => mqt.TrackingStatusId == trackingStatusId));
            }

            var statusFlags = DbMetaTag.StatusFlags;
            var requestTypes = DbMetaTag.RequestTypes;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "none")
                    dbQuery = dbQuery.Where(mq => !mq.MetaQuestionMetaTags.Any(mqt => statusFlags.Contains(mqt.TagName)) && !mq.ClosedDate.HasValue);
                else if (status == "closed")
                    dbQuery = dbQuery.Where(mq => mq.ClosedDate.HasValue);
                else
                    dbQuery = dbQuery.Where(mq => mq.MetaQuestionMetaTags.Any(mqt => mqt.TagName == status));
            }

            if (!string.IsNullOrWhiteSpace(requestType))
            {
                dbQuery = dbQuery.Where(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == requestType));
            }

            var transformedQuery = dbQuery
                .Where(mq => mq.MetaQuestionMetaTags.Any(mqt => DbMetaTag.RequestTypes.Contains(mqt.TagName)))
                .Select(mq => new
                {
                    mq.Id,
                    mq.Title,
                    mq.Score,
                    MainTags = mq.MetaQuestionTags.Select(mqt => new
                    {
                        mqt.TagName,
                        mqt.TrackingStatusId,
                        TrackingStatusName = mqt.TrackingStatus.Name,
                        NumQuestions = mqt.Tag.NumberOfQuestions,
                        SynonymOf = mqt.Tag.SynonymOfTagName,
                        HasQuestionCountOverTimeData = mqt.Tag.Statistics.Count > 1
                    }),
                    MetaStatusTags = mq.MetaQuestionMetaTags.Where(mqmt => statusFlags.Contains(mqmt.TagName)).Select(mqt => new
                    {
                        mqt.TagName
                    }),
                    MetaRequestTags = mq.MetaQuestionMetaTags.Where(mqmt => requestTypes.Contains(mqmt.TagName)).Select(mqt => new
                    {
                        mqt.TagName
                    }),
                    NumQuestions = mq.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED)
                        .Select(mqt => mqt.Tag.NumberOfQuestions)
                        .OrderByDescending(mqt => mqt)
                        .FirstOrDefault(),
                    Closed = mq.ClosedDate.HasValue,

                    mq.FeaturedStarted,
                    mq.FeaturedEnded,
                    mq.BurnStarted,
                    mq.BurnEnded
                });

            if (hasQuestions == "yes")
                transformedQuery = transformedQuery.Where(tq => tq.NumQuestions > 0);
            if (hasQuestions == "no")
                transformedQuery = transformedQuery.Where(tq => tq.NumQuestions <= 0);

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

        [HttpGet("QuestionCountOverTime")]
        public object GetQuestionCountOverTime(string tag)
        {
            return _context.Tags.Where(t => t.Name == tag)
                .Select(t => new
                {
                    QuestionCountOverTime = t.Statistics.Select(s => new {s.DateTime, s.QuestionCount}).OrderBy(s => s.DateTime)
                })
                .FirstOrDefault();
        }

        [HttpPost("SetTagTrackingStatus")]
        [Authorize]
        public void SetTagTrackingStatus([FromBody] SetTagTrackingStatusRequest request)
        {
            if (!User.HasRole(DbRole.TRIAGER))
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var matchingQuestionMetaTag = _context.MetaQuestionTags.FirstOrDefault(mqt => mqt.MetaQuestionId == request.MetaQuestionId && mqt.TagName == request.TagName);
            if (matchingQuestionMetaTag != null)
            {
                var newStatusId =
                    request.Tracked
                        ? DbMetaQuestionTagTrackingStatus.TRACKED
                        : DbMetaQuestionTagTrackingStatus.IGNORED;

                if (matchingQuestionMetaTag.TrackingStatusId != newStatusId)
                {
                    _context.MetaQuestionTagTrackingStatusAudits.Add(new DbMetaQuestionTagTrackingStatusAudit
                    {
                        Tag = request.TagName,
                        MetaQuestionId = request.MetaQuestionId,
                        ChangedByUserId = User.UserId(),
                        PreviousTrackingStatusId = matchingQuestionMetaTag.TrackingStatusId,
                        NewTrackingStatusId = newStatusId,
                        TimeChanged = _dateService.UtcNow
                    });

                    matchingQuestionMetaTag.TrackingStatusId = newStatusId;
                    
                    _context.SaveChanges();
                }
            }
        }

        public class SetTagTrackingStatusRequest
        {
            public int MetaQuestionId { get; set; }
            public string TagName { get; set; }
            public bool Tracked { get; set; }
        }
        
        [HttpPost("AddTag")]
        [Authorize]
        public void AddTag([FromBody] AddTagRequest request)
        {
            if (!User.HasRole(DbRole.TRIAGER))
                throw new HttpStatusException(HttpStatusCode.Forbidden);
            
            var matchingQuestionMetaTag = _context.MetaQuestions
                .Include(mq => mq.MetaQuestionTags)
                .FirstOrDefault(mqt => mqt.Id == request.MetaQuestionId);

            if (matchingQuestionMetaTag != null)
            {
                var alreadyExistingTag = matchingQuestionMetaTag.MetaQuestionTags.FirstOrDefault(mqt => mqt.TagName == request.TagName);
                if (alreadyExistingTag != null)
                {
                    _context.MetaQuestionTagTrackingStatusAudits.Add(new DbMetaQuestionTagTrackingStatusAudit
                    {
                        Tag = request.TagName,
                        MetaQuestionId = request.MetaQuestionId,
                        ChangedByUserId = User.UserId(),
                        PreviousTrackingStatusId = alreadyExistingTag.TrackingStatusId,
                        NewTrackingStatusId = DbMetaQuestionTagTrackingStatus.TRACKED,
                        TimeChanged = _dateService.UtcNow
                    });

                    alreadyExistingTag.TrackingStatusId = DbMetaQuestionTagTrackingStatus.TRACKED;
                }
                else
                {
                    var tagExists = _context.Tags.Any(mt => mt.Name == request.TagName);
                    if (!tagExists)
                        _context.Tags.Add(new DbTag { Name = request.TagName });

                    _context.MetaQuestionTags.Add(new DbMetaQuestionTag
                    {
                        MetaQuestionId = request.MetaQuestionId,
                        TrackingStatusId = DbMetaQuestionTagTrackingStatus.TRACKED,
                        TagName = request.TagName
                    });

                    _context.MetaQuestionTagTrackingStatusAudits.Add(new DbMetaQuestionTagTrackingStatusAudit
                    {
                        Tag = request.TagName,
                        MetaQuestionId = request.MetaQuestionId,
                        ChangedByUserId = User.UserId(),
                        PreviousTrackingStatusId = null,
                        NewTrackingStatusId = DbMetaQuestionTagTrackingStatus.TRACKED,
                        TimeChanged = _dateService.UtcNow
                    });
                }

                _context.SaveChanges();
            }
        }

        public class AddTagRequest
        {
            public int MetaQuestionId { get; set; }
            public string TagName { get; set; }
        }
    }
}