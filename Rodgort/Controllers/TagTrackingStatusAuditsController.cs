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
    public class TagTrackingStatusAuditsController : ControllerBase
    {
        private readonly RodgortContext _context;

        public TagTrackingStatusAuditsController(RodgortContext context)
        {
            _context = context;
        }

        [HttpGet]
        public object Get(
            int? userId = null, 
            int? metaQuestionId = null,
            int page = 1, int pageSize = 30)
        {
            IQueryable<DbMetaQuestionTagTrackingStatusAudit> query = _context.MetaQuestionTagTrackingStatusAudits;
            if (userId.HasValue)
                query = query.Where(q => q.ChangedByUserId == userId.Value);
            if (metaQuestionId.HasValue)
                query = query.Where(q => q.MetaQuestionId == metaQuestionId.Value);

            return query
                .Select(audit => new
                {
                    audit.TimeChanged,
                    UserId = audit.ChangedByUserId,
                    UserName = audit.ChangedByUser.DisplayName,
                    audit.ChangedByUser.IsModerator,
                    audit.MetaQuestionId,
                    MetaQuestionTitle = audit.MetaQuestion.Title,
                    audit.Tag,
                    PreviousStatusId = audit.PreviousTrackingStatusId,
                    PreviousStatus = audit.PreviousTrackingStatus.Name,
                    NewStatusId = audit.NewTrackingStatusId,
                    NewStatus = audit.NewTrackingStatus.Name
                })
                .OrderByDescending(a => a.TimeChanged)
                .Page(page, pageSize);
        }
    }
}