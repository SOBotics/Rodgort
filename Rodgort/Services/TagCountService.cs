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
        public const string ACTIVE_TAGS = "Fetch question counts per tag last seen with at least one question";
        public const string EMPTY_TAGS = "Fetch question counts for tags last seen with zero questions";
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

        public async Task GetQuestionCountForActiveTags()
        {
            var tagsToCheck = _context.MetaQuestionTags
                .Where(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED)
                // If the request was declined, we don't need to watch the count
                .Where(mqt => mqt.MetaQuestion.MetaQuestionMetaTags.All(mqmt => mqmt.TagName != DbMetaTag.STATUS_DECLINED))
                .Where(mqt => !mqt.Tag.NumberOfQuestions.HasValue || mqt.Tag.NumberOfQuestions.Value > 0)
                .Select(mqt => mqt.Tag)
                .Distinct()
                .ToList();

            await ProcessTags(tagsToCheck);
        }


        public async Task GetQuestionCountForEmptyTags()
        {
            var tagsToCheck = _context.MetaQuestionTags
                .Where(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED)
                // If the request was declined, we don't need to watch the count
                .Where(mqt => mqt.MetaQuestion.MetaQuestionMetaTags.All(mqmt => mqmt.TagName != DbMetaTag.STATUS_DECLINED))
                .Where(mqt => mqt.Tag.NumberOfQuestions.HasValue && mqt.Tag.NumberOfQuestions == 0)
                .Select(mqt => mqt.Tag)
                .Distinct()
                .ToList();

            await ProcessTags(tagsToCheck);
        }


        public async Task GetQuestionCountForFeaturedOrInProgressBurninations()
        {
            var tagsToCheck = _context.MetaQuestionTags
                .Where(mqt => mqt.MetaQuestion.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_PLANNED)
                    || mqt.MetaQuestion.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_FEATURED))
                .Where(mqt => mqt.StatusId == DbMetaQuestionTagStatus.APPROVED)
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

                _context.TagStatistics.Add(new DbTagStatistics
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
