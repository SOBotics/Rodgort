using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rodgort.Data;

namespace Rodgort.Services
{
    public class InitialTagQueryService
    {
        private readonly RodgortContext _context;
        private readonly TagCountService _tagCountService;
        private readonly ILogger<InitialTagQueryService> _logger;

        public const string SERVICE_NAME = "Initial tag query service";
        
        public InitialTagQueryService(
            RodgortContext context,
            TagCountService tagCountService,
            ILogger<InitialTagQueryService> logger)
        {
            _context = context;
            _tagCountService = tagCountService;
            _logger = logger;
        }

        public void QuerySync()
        {
            Query().Wait();
        }

        private async Task Query()
        {
            _logger.LogInformation("Beginning to query tags with no statistics");
            var tagsWithNoStats = _context.Tags.Where(t => !t.Statistics.Any()).ToList();

            await _tagCountService.ProcessTags(tagsWithNoStats);

            _logger.LogInformation("Finished querying tags with no statistics");
        }
    }
}
