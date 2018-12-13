using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
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
        public object Get(int page = 1, int pageSize = 30)
        {
            var result = _context.MetaQuestions
                .Where(q => q.Id == 320690)
                .Select(mq => new
            {
                mq.Id,
                mq.Title,
                QuestionsInTag = 5,
                MainTags = mq.MetaQuestionTags.Select(mqt => new
                {
                    mqt.TagName,
                    Type = mqt.RequestType.Name,
                    Status = mqt.Status.Name,
                    QuestionCountOverTime = mqt.Tag.Statistics.Select(s => new { s.DateTime, s.QuestionCount })
                }),
                ScoreOverTime = mq.Statistics.Select(s => new { s.DateTime, s.Score}),
            })
            .OrderBy(q => q.Id)
            .Page(page, pageSize);
               
            return result;
        }
    }
}