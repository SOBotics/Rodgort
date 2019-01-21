using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        public object Get(string search, string level, int page = 1, int pageSize = 30)
        {
            IQueryable<DbLog> query = _context.Logs;
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(q => q.Message.Contains(search) || q.Exception.Contains(search));
            if (!string.IsNullOrWhiteSpace(level))
            {
                var enumList = new[] {"Trace", "Debug", "Info", "Warn", "Error", "Fatal"}.Select((e, i) => new {Name = e, Value = i}).ToList();

                var matchedLevel = enumList.First(e => e.Name == level) ?? enumList.First();
                var desiredLevel = matchedLevel.Value;

                var lookFor = enumList.Where(e => e.Value > desiredLevel).Select(e => e.Name).ToList();
                query = query.Where(q => lookFor.Contains(q.Level));
            }
            var orderedQuery = query.OrderByDescending(l => l.TimeLogged);
            var result = orderedQuery.Page(page, pageSize);
            return result;
        }
    }
}