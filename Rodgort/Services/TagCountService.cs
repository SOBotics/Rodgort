using System.Collections.Generic;
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
        public const string QUESTION_COUNT_SERVICE_NAME = "Fetch question counts per tag";
        public const string IN_PROGRESS = "Fetch question counts per tag for burninations in progress";

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
                .Where(mqt => !mqt.MetaQuestion.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_COMPLETED || mqmt.TagName == DbMetaTag.STATUS_DECLINED))
                .Select(mqt => mqt.Tag)
                .Distinct()
                .ToList();

            await ProcessTags(tagsToCheck);
        }

        public async Task GetQuestionCountForInProgressBurninations()
        {
            var tagsToCheck = _context.MetaQuestionTags
                .Where(mqt => mqt.MetaQuestion.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_PLANNED))
                .Where(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED)
                .Where(mqt => !mqt.MetaQuestion.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_COMPLETED || mqmt.TagName == DbMetaTag.STATUS_DECLINED))
                .Select(mqt => mqt.Tag)
                .Distinct()
                .ToList();

            await ProcessTags(tagsToCheck);
        }

        private async Task ProcessTags(List<DbTag> tagsToCheck)
        {
            _logger.LogInformation("Fetching question counts for the following tags: " + string.Join(", ", tagsToCheck));
            foreach (var tagToCheck in tagsToCheck)
            {
                var tagName = tagToCheck.Name;
                var response = await _apiClient.TotalQuestionsByTag("stackoverflow.com", tagName);

                _context.DbTagStatistics.Add(new DbTagStatistics
                {
                    QuestionCount = response.Total,
                    DateTime = _dateService.UtcNow,
                    TagName = tagName
                });
                tagToCheck.NumberOfQuestions = response.Total;
            }

            _context.SaveChanges();
            _logger.LogInformation("Finished fetching question counts");
        }
    }
}
