using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rodgort.Data;
using Rodgort.Data.Views;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class ZombieController : Controller
    {
        private readonly RodgortContext _context;

        public ZombieController(RodgortContext context)
        {
            _context = context;
        }

        [HttpGet]
        public object Index(string tag, bool onlyAlive)
        {
            IQueryable<DbZombieTagsView> zombieQuery = _context.ZombieTagsView;
            if (!string.IsNullOrWhiteSpace(tag))
                zombieQuery = zombieQuery.Where(z => z.TagName == tag);
            if (onlyAlive)
                zombieQuery = zombieQuery.Where(z => z.Tag.NumberOfQuestions > 0);
            
            var zombies = zombieQuery
                .Include(z => z.Tag.Statistics)
                .ToList();

            var result = zombies
                .GroupBy(z => z.Tag)
                .Select(g => new
                {
                    Tag = g.Key.Name,
                    Revivals = g.Select(tt => tt.TimeRevived).ToList(),
                    QuestionCountOverTime = g.First().Tag.Statistics
                        .Select(stat => new
                        {
                            stat.QuestionCount,
                            stat.DateTime
                        })
                        .OrderByDescending(s => s.DateTime)
                }).ToList();

            return result;
        }
    }
}