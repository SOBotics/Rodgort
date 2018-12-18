using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoreLinq;
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

        private async Task ProcessTags(IReadOnlyCollection<DbTag> tagsToCheck)
        {
            _logger.LogInformation("Fetching question counts for the following tags: " + string.Join(", ", tagsToCheck));

            foreach (var batch in tagsToCheck.Batch(95))
            {
                var tagLookup = batch.ToDictionary(b => b.Name, tag => tag, StringComparer.OrdinalIgnoreCase);
                
                var response = await _apiClient.TotalQuestionsByTag("stackoverflow.com", tagLookup.Keys);

                foreach (var responseTag in response.Items)
                {
                    if (tagLookup.ContainsKey(responseTag.Name))
                    {
                        _context.TagStatistics.Add(new DbTagStatistics
                        {
                            QuestionCount = responseTag.Count,
                            DateTime = _dateService.UtcNow,
                            TagName = responseTag.Name
                        });

                        tagLookup[responseTag.Name].NumberOfQuestions = responseTag.Count;
                    }

                    
                    if (responseTag.Synonyms != null)
                    {
                        foreach (var synonymTagName in responseTag.Synonyms)
                        {
                            if (tagLookup.ContainsKey(synonymTagName))
                            {
                                var synonym = tagLookup[synonymTagName];
                                synonym.SynonymOfTagName = responseTag.Name;
                                synonym.NumberOfQuestions = 0;

                                _context.TagStatistics.Add(new DbTagStatistics
                                {
                                    QuestionCount = 0,
                                    DateTime = _dateService.UtcNow,
                                    TagName = synonymTagName
                                });
                            }
                        }
                    }
                }
            }

            var currentTags = tagsToCheck.Select(mqmt => mqmt.SynonymOfTagName).Where(n => n != null).Distinct().ToList();
            var dbTags = _context.Tags.Where(t => currentTags.Contains(t.Name)).ToLookup(t => t.Name);
            foreach (var currentTag in currentTags)
            {
                if (!dbTags.Contains(currentTag))
                    _context.Tags.Add(new DbTag { Name = currentTag });
            }

            _context.SaveChanges();
            _logger.LogInformation("Finished fetching question counts");
        }
    }
}
