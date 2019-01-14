using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rodgort.ApiUtilities;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Utilities;
using StackExchangeApi;
using StackExchangeApi.Responses;
using StackExchangeChat;

namespace Rodgort.Services
{
    public class MetaCrawlerService
    {
        public const string SERVICE_NAME = "Refresh burnination request list";

        private readonly DbContextOptions<RodgortContext> _dbContextOptions;
        private readonly ApiClient _apiClient;
        private readonly DateService _dateService;
        private readonly ILogger<MetaCrawlerService> _logger;
        private readonly NewBurninationService _newBurninationService;

        private static readonly object _locker = new object();
        private static bool _alreadyProcessing;

        public MetaCrawlerService(DbContextOptions<RodgortContext> dbContextOptions, 
            ApiClient apiClient, 
            DateService dateService, 
            ILogger<MetaCrawlerService> logger,
            NewBurninationService newBurninationService)
        {
            _dbContextOptions = dbContextOptions;
            _apiClient = apiClient;
            _dateService = dateService;
            _logger = logger;
            _newBurninationService = newBurninationService;
        }

        private class NewFeature
        {
            public string MetaUrl { get; set; }
            public List<string> Tags { get; set; }
        }

        private class BurnFinished
        {
            public string Tag { get; set; }
        }

        public void CrawlMetaSync()
        {
            CrawlMeta().Wait();
        }

        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField", Justification = "Flagged _logger, but in both blocks we're guaranteed to run on a single thread")]
        private async Task CrawlMeta()
        {
            lock (_locker)
            {
                if (_alreadyProcessing)
                {
                    _logger.LogWarning("Attempted to crawl meta while process already running");
                    return;
                }
                    
                _alreadyProcessing = true;
            }
            
            try
            {
                var newFeatures = new List<NewFeature>();
                var finishedBurns = new List<BurnFinished>();

                _logger.LogInformation("Starting meta crawl");

                foreach (var tagToCrawl in DbMetaTag.RequestTypes)
                {
                    _logger.LogInformation($"Crawling {tagToCrawl}");

                    var metaQuestions = await _apiClient.MetaQuestionsByTag("meta.stackoverflow.com", tagToCrawl);

                    _logger.LogInformation($"{metaQuestions.Items.Count} meta questions retrieved for {tagToCrawl}. Processing...");

                    var questionIds = metaQuestions.Items.Select(q => q.QuestionId).Distinct().ToList();

                    var context = new RodgortContext(_dbContextOptions);

                    var questionLookup = context.MetaQuestions.Where(q => questionIds.Contains(q.Id))    
                        .Include(mq => mq.MetaQuestionMetaTags)
                        .Include(mq => mq.MetaQuestionTags)
                        .ToDictionary(q => q.Id, q => q);

                    var answerIds = metaQuestions.Items.Where(q => q.Answers != null).SelectMany(q => q.Answers.Select(a => a.AnswerId)).Distinct().ToList();
                    var answerLookup = context.MetaAnswers.Where(q => answerIds.Contains(q.Id)).ToDictionary(a => a.Id, a => a);

                    var utcNow = _dateService.UtcNow;
                    foreach (var metaQuestion in metaQuestions.Items)
                    {
                        DbMetaQuestion dbMetaQuestion;
                        if (!metaQuestion.QuestionId.HasValue)
                            throw new InvalidOperationException($"Question object does not contain {nameof(metaQuestion.QuestionId)}");

                        if (!questionLookup.ContainsKey(metaQuestion.QuestionId.Value))
                        {
                            dbMetaQuestion = new DbMetaQuestion { Id = metaQuestion.QuestionId.Value, MetaQuestionTags = new List<DbMetaQuestionTag>() };
                            context.MetaQuestions.Add(dbMetaQuestion);

                            questionLookup[dbMetaQuestion.Id] = dbMetaQuestion;
                        }
                        else
                        {
                            dbMetaQuestion = questionLookup[metaQuestion.QuestionId.Value];
                        }

                        if (!metaQuestion.Score.HasValue)
                            throw new InvalidOperationException($"Question object does not contain {nameof(metaQuestion.Score)}");
                        if (!metaQuestion.ViewCount.HasValue)
                            throw new InvalidOperationException($"Question object does not contain {nameof(metaQuestion.ViewCount)}");

                        dbMetaQuestion.Title = WebUtility.HtmlDecode(metaQuestion.Title);
                        dbMetaQuestion.Body = WebUtility.HtmlDecode(metaQuestion.BodyMarkdown);
                        dbMetaQuestion.Link = metaQuestion.Link;
                        dbMetaQuestion.LastSeen = utcNow;
                        dbMetaQuestion.Score = metaQuestion.Score.Value;
                        dbMetaQuestion.ViewCount = metaQuestion.ViewCount.Value;
                        dbMetaQuestion.CloseReason = metaQuestion.ClosedReason;

                        if (metaQuestion.ClosedDate.HasValue)
                            dbMetaQuestion.ClosedDate = Dates.UnixTimeStampToDateTime(metaQuestion.ClosedDate.Value);

                        var trackedTags = dbMetaQuestion.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED).ToList();
                        foreach (var tag in metaQuestion.Tags)
                        {
                            if (!dbMetaQuestion.MetaQuestionMetaTags.Any(t => string.Equals(t.TagName, tag, StringComparison.OrdinalIgnoreCase)))
                            {
                                context.MetaQuestionMetaTags.Add(new DbMetaQuestionMetaTag { TagName = tag, MetaQuestion = dbMetaQuestion });
                                if (tag == DbMetaTag.STATUS_FEATURED)
                                {
                                    dbMetaQuestion.FeaturedStarted = utcNow;
                                    newFeatures.Add(new NewFeature { MetaUrl = metaQuestion.Link, Tags = trackedTags.Select(t => t.TagName).ToList() });
                                }
                                if (tag == DbMetaTag.STATUS_PLANNED)
                                    dbMetaQuestion.BurnStarted = utcNow;
                            }
                        }

                        var metaQuestionList = dbMetaQuestion.MetaQuestionMetaTags.ToList();
                        foreach (var dbTag in metaQuestionList)
                        {
                            if (!metaQuestion.Tags.Any(t => string.Equals(t, dbTag.TagName, StringComparison.OrdinalIgnoreCase)))
                            {
                                context.MetaQuestionMetaTags.Remove(dbTag);

                                if (dbTag.TagName == DbMetaTag.STATUS_FEATURED)
                                    dbMetaQuestion.FeaturedEnded = utcNow;
                                if (dbTag.TagName == DbMetaTag.STATUS_PLANNED)
                                {
                                    dbMetaQuestion.BurnEnded = utcNow;
                                    foreach (var trackedTag in trackedTags)
                                        finishedBurns.Add(new BurnFinished {Tag = trackedTag.TagName });
                                }
                            }
                        }

                        var dbMetaQuestionStatistics = new DbMetaQuestionStatistics
                        {
                            DateTime = utcNow,
                            Score = metaQuestion.Score.Value,
                            ViewCount = metaQuestion.ViewCount.Value,

                            MetaQuestion = dbMetaQuestion
                        };
                        context.Add(dbMetaQuestionStatistics);

                        foreach (var metaAnswer in metaQuestion.Answers ?? Enumerable.Empty<BaseAnswer>())
                        {
                            DbMetaAnswer dbMetaAnswer;
                            if (!metaAnswer.AnswerId.HasValue)
                                throw new InvalidOperationException($"Answer object does not contain {nameof(metaAnswer.AnswerId)}");

                            if (!answerLookup.ContainsKey(metaAnswer.AnswerId.Value))
                            {
                                dbMetaAnswer = new DbMetaAnswer { Id = metaAnswer.AnswerId.Value };
                                context.MetaAnswers.Add(dbMetaAnswer);

                                answerLookup[dbMetaAnswer.Id] = dbMetaAnswer;
                            }
                            else
                            {
                                dbMetaAnswer = answerLookup[metaAnswer.AnswerId.Value];
                            }

                            dbMetaAnswer.Body = metaAnswer.BodyMarkdown;
                            dbMetaAnswer.MetaQuestion = dbMetaQuestion;
                            dbMetaAnswer.LastSeen = utcNow;

                            if (!metaAnswer.Score.HasValue)
                                throw new InvalidOperationException($"Answer object does not contain {nameof(metaAnswer.Score)}");

                            var dbMetaAnswerStatistics = new DbMetaAnswerStatistics
                            {
                                DateTime = utcNow,
                                Score = metaAnswer.Score.Value,

                                MetaAnswer = dbMetaAnswer
                            };

                            context.Add(dbMetaAnswerStatistics);
                        }

                        var currentTags = context.MetaQuestionMetaTags.Local.Select(mqmt => mqmt.TagName).Distinct().ToList();
                        var dbTags = context.MetaTags.Where(t => currentTags.Contains(t.Name)).ToLookup(t => t.Name);
                        foreach (var currentTag in currentTags)
                        {
                            if (!dbTags.Contains(currentTag))
                                context.MetaTags.Add(new DbMetaTag { Name = currentTag });
                        }

                        context.SaveChanges();
                    }
                }

                foreach (var finishedBurn in finishedBurns.Distinct())
                    await _newBurninationService.StopBurn(finishedBurn.Tag);

                foreach (var newFeature in newFeatures.GroupBy(g => g.MetaUrl).Select(g => g.First()))
                {
                    if (!newFeature.Tags.Any())
                    {
                        await _newBurninationService.AnnounceNoTrackedTags(newFeature.MetaUrl);
                    }
                    else if (newFeature.Tags.Count > 1)
                    {
                        await _newBurninationService.AnnounceMultipleTrackedTags(newFeature.MetaUrl, newFeature.Tags);
                    }
                    else
                    {
                        await _newBurninationService.CreateRoomForBurn(newFeature.Tags.First(), newFeature.MetaUrl);
                    }
                }

                _logger.LogInformation("Meta crawl completed");
                RecurringJob.Trigger(BurninationTagGuessingService.SERVICE_NAME);
            }
            finally
            {
                lock (_locker)
                    _alreadyProcessing = false;
            }
        }
    }
}
