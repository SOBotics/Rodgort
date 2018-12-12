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
            var result = _context.MetaQuestions.Select(mq => new
            {
                mq.Id,
                mq.Title,
                QuestionsInTag = 5,
                MainTags = new[] {new {TagName = "design", Status = "guessed"}}
            }).Page(page, pageSize);
               
            return result;
        }
    }
}