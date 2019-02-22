using System;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rodgort.Data;

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
        public object Index(bool onlyAlive)
        {
            var zombies = _context.Database.GetDbConnection()
                .Query<ZombieQuery>(@"
select 
	tag_name as Tag,
	date_time DateRevived
 from (
	select 
		a.*,
		case 
			when lag(question_count) over (partition by a.tag_name order by date_time) = 0 and question_count > 0 then true
			else false
		end 
		as revived
	from 
    tags t
    inner join tag_statistics a on t.name = a.tag_name
	inner join meta_question_tags mqt on mqt.tag_name = t.name and mqt.tracking_status_id = 2
    where @allZombies or t.number_of_questions > 0 
) innerQuery
where innerQuery.revived
", new
                {
                    allZombies = !onlyAlive
                }).ToList();

            var zombiedTags = zombies.Select(z => z.Tag).Distinct();

            var questionCountsOverTime = _context.TagStatistics.Where(t => zombiedTags.Contains(t.TagName))
                .ToList()
                .GroupBy(t => t.TagName)
                .ToDictionary(t => t.Key);

            var result = zombies
                .GroupBy(z => z.Tag)
                .Select(t => new
                {
                    Tag = t.Key,
                    Revivals = t.Select(tt => tt.DateRevived).ToList(),
                    QuestionCountOverTime = questionCountsOverTime[t.Key]
                })
                .OrderByDescending(r => r.Revivals.Count)
                .ToList();

            return result;
        }

        private class ZombieQuery
        {
            public string Tag { get; set; }
            public DateTime DateRevived { get; set; }
        }
    }
}