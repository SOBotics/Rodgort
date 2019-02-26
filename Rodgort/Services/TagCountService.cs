using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rodgort.Data;
using Rodgort.Data.Tables;
using StackExchangeApi;

namespace Rodgort.Services
{
    public class TagCountService
    {
        public const string ALL_TAGS = "Fetch question counts per tag";
        
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

        public void GetQuestionCountForApprovedTagsSync()
        {
            GetQuestionCountForApprovedTags().Wait();
        }

        private async Task GetQuestionCountForApprovedTags()
        {
            var tagsToCheck = _context.MetaQuestionTags
                // The tag was approved
                .Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE)
                // If the request was declined, we don't need to watch the count
                .Where(mqt => mqt.MetaQuestion.MetaQuestionMetaTags.All(mqmt => mqmt.TagName != DbMetaTag.STATUS_DECLINED))
                .Select(mqt => mqt.Tag)

                // Don't check the counts for synonymised tags.
                // SE sometimes incorrectly returns the synonymised tag instead of the master
                // This causes spikes in our graphs, alternating between the actual count, and zero.

                .Where(tag => tag.SynonymOf == null) 
                .Distinct()
                .ToList();

            await ProcessTags(tagsToCheck);
        }

        private IEnumerable<IEnumerable<DbTag>> BatchTags(IReadOnlyCollection<DbTag> tagsToCheck)
        {
            const int maxSize = 100;
            const int maxLength = 908;

            var currentList = new List<DbTag>();

            foreach (var tagToCheck in tagsToCheck)
            {
                if (
                    string.Join(";", currentList.Concat(new[] {tagToCheck}).Select(t => t.Name)).Length > maxLength
                    || currentList.Count >= maxSize)
                {
                    yield return currentList;
                    currentList = new List<DbTag>();
                }

                currentList.Add(tagToCheck);
            }

            if (currentList.Any())
                yield return currentList;
        }

        public async Task ProcessTags(IReadOnlyCollection<DbTag> tagsToCheck)
        {
            _logger.LogInformation($"Fetching question counts for {tagsToCheck.Count} tags.");

            foreach (var batch in BatchTags(tagsToCheck))
            {
                var tagLookup = batch.ToDictionary(b => b.Name, tag => tag, StringComparer.OrdinalIgnoreCase);
                
                var response = await _apiClient.TotalQuestionsByTag("stackoverflow.com", tagLookup.Keys);

                var responseLookup = response.Items.ToLookup(r => r.Name);
                foreach (var responseTag in response.Items)
                {
                    if (tagLookup.ContainsKey(responseTag.Name))
                    {   
                        _context.TagStatistics.Add(new DbTagStatistics
                        {
                            QuestionCount = responseTag.Count,
                            DateTime = _dateService.UtcNow,
                            TagName = responseTag.Name,
                            IsSynonym = false
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
                                    TagName = synonymTagName,
                                    IsSynonym = true
                                });
                            }
                        }
                    }
                }

                foreach (var tag in tagLookup.Values)
                {
                    if (responseLookup.Contains(tag.Name))
                        continue;

                    // If it's got a synonym, we've already handled the 0 entry
                    if (!string.IsNullOrWhiteSpace(tag.SynonymOfTagName))
                        continue;

                    tag.NumberOfQuestions = 0;
                    _context.TagStatistics.Add(new DbTagStatistics
                    {
                        QuestionCount = 0,
                        DateTime = _dateService.UtcNow,
                        TagName = tag.Name,
                        IsSynonym = true
                    });
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

            _context.Database.ExecuteSqlCommand("REFRESH MATERIALIZED VIEW zombie_tags");

            _logger.LogInformation("Finished fetching question counts");
        }
    }
}
