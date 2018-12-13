using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Utilities.Paging;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class LogsController : Controller
    {
        private readonly RodgortContext _context;

        public LogsController(RodgortContext context)
        {
            _context = context;
        }

        [HttpGet]
        public object Get(int page = 1, int pageSize = 30)
        {
            var query = _context.Logs.OrderByDescending(l => l.TimeLogged);
            var result = query.Page(page, pageSize);
            return result;
        }
    }
}