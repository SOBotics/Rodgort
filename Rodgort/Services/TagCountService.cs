using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rodgort.Data;
using Rodgort.Data.Tables;
using StackExchangeApi;

namespace Rodgort.Services
{
    public class TagCountService
    {
        public const string SERVICE_NAME = "Fetch question counts per tag";

        private readonly RodgortContext _context;
        private readonly ILogger<TagCountService> _logger;
        private readonly ApiClient _apiClient;
        private readonly DateService _dateService;

        public TagCountService(RodgortContext context, 
            ILogger<TagCountService> logger,
            ApiClient apiClient,
            DateService dateService)
        {
            _context = context;
            _logger = logger;
            _apiClient = apiClient;
            _dateService = dateService;
        }

        public async Task GetQuestionCount()
        {
            var tagsToCheck = _context.MetaQuestionTags
                .Where(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED)
                .Select(mqt => mqt.TagName)
                .Distinct()
                .ToList();

            _logger.LogInformation("Fetching question counts for the following tags: ", string.Join(", ", tagsToCheck));
            foreach (var tagToCheck in tagsToCheck)
            {
                var response = await _apiClient.TotalQuestionsByTag("stackoverflow.com", tagToCheck);

                _context.DbTagStatistics.Add(new DbTagStatistics
                {
                    QuestionCount = response.Total,
                    DateTime = _dateService.UtcNow,
                    TagName = tagToCheck
                });
            }

            _context.SaveChanges();
            _logger.LogInformation("Finished fetching question counts");
        }
    }
}
