using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rodgort.ApiUtilities;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Utilities;
using StackExchangeApi;
using StackExchangeApi.Responses;

namespace Rodgort.Services
{
    public class MetaCrawlerService
    {
        public const string SERVICE_NAME = "Refresh burnination request list";

        private readonly DbContextOptions<RodgortContext> _dbContextOptions;
        private readonly BurninationTagGuessingService _tagGuessingService;
        private readonly ApiClient _apiClient;
        private readonly DateService _dateService;
        private readonly ILogger<MetaCrawlerService> _logger;
        private readonly NewBurninationService _newBurninationService;

        private static readonly object _locker = new object();
        private static bool _alreadyProcessing;

        public MetaCrawlerService(DbContextOptions<RodgortContext> dbContextOptions,
            BurninationTagGuessingService tagGuessingService,
            ApiClient apiClient, 
            DateService dateService, 
            ILogger<MetaCrawlerService> logger,
            NewBurninationService newBurninationService)
        {
            _dbContextOptions = dbContextOptions;
            _tagGuessingService = tagGuessingService;
            _apiClient = apiClient;
            _dateService = dateService;
            _logger = logger;
            _newBurninationService = newBurninationService;
        }

        public class NewFeature
        {
            public int MetaQuestionId { get; set; }
            public string MetaUrl { get; set; }
            public List<string> Tags { get; set; }
        }

        public class BurnFinished
        {
            public string Tag { get; set; }
        }

        public class BurnStarted
        {
            public string MetaUrl { get; set; }
            public List<string> Tags { get; set; }
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
                var burnsStarted = new List<BurnStarted>();

                _logger.LogInformation("Starting meta crawl");

                var questions = new List<BaseQuestion>();

                foreach (var tagToCrawl in DbMetaTag.RequestTypes)
                {
                    _logger.LogInformation($"Crawling {tagToCrawl}");

                    var metaQuestions = await _apiClient.MetaQuestionsByTag("meta.stackoverflow.com", tagToCrawl);
                    questions.AddRange(metaQuestions.Items);

                    _logger.LogInformation($"{metaQuestions.Items.Count} meta questions retrieved for {tagToCrawl}. Processing...");

                    var result = ProcessQuestions(metaQuestions.Items);
                    newFeatures.AddRange(result.NewFeatures);
                    finishedBurns.AddRange(result.FinishedBurns);
                    burnsStarted.AddRange(result.BurnsStarted);
                }

                await PostProcessQuestions(questions, finishedBurns, newFeatures, burnsStarted);
            }
            finally
            {
                lock (_locker)
                    _alreadyProcessing = false;
            }
        }

        public async Task PostProcessQuestions(List<BaseQuestion> questions, ProcessQuestionsResult processQuestionsResult)
        {
            await PostProcessQuestions(questions, processQuestionsResult.FinishedBurns, processQuestionsResult.NewFeatures, processQuestionsResult.BurnsStarted);
        }

        public async Task PostProcessQuestions(List<BaseQuestion> questions, IEnumerable<BurnFinished> finishedBurns, IReadOnlyCollection<NewFeature> newFeatures, IReadOnlyCollection<BurnStarted> burnsStarted)
        {
            foreach (var tag in finishedBurns.Select(b => b.Tag).Distinct())
                await _newBurninationService.StopBurn(tag);

            foreach (var newFeature in newFeatures.GroupBy(g => g.MetaUrl).Select(g => g.First()))
                await _newBurninationService.NewTagsFeatured(newFeature.MetaQuestionId, newFeature.MetaUrl, newFeature.Tags);

            foreach (var burnStarted in burnsStarted.GroupBy(g => g.MetaUrl).Select(g => g.First()))
                await _newBurninationService.NewBurnStarted(burnStarted.MetaUrl, burnStarted.Tags);

            _logger.LogInformation("Meta crawl completed");
            
            _tagGuessingService.GuessTags(questions.Where(q => q.QuestionId.HasValue).Select(q => q.QuestionId.Value));

            if (newFeatures.Any() || burnsStarted.Any())
                RecurringJob.Trigger(BurnCatchupService.SERVICE_NAME);
        }

        public class ProcessQuestionsResult
        {
            public List<NewFeature> NewFeatures { get; set; }
            public List<BurnFinished> FinishedBurns { get; set; }
            public List<BurnStarted> BurnsStarted { get; set; }
        }

        public ProcessQuestionsResult ProcessQuestions(List<BaseQuestion> questions)
        {
            var newFeatures = new List<NewFeature>();
            var finishedBurns = new List<BurnFinished>();
            var burnsStarted = new List<BurnStarted>();

            var questionIds = questions.Select(q => q.QuestionId).Distinct().ToList();

            var context = new RodgortContext(_dbContextOptions);

            var questionLookup = context.MetaQuestions.Where(q => questionIds.Contains(q.Id))
                .Include(mq => mq.MetaQuestionMetaTags)
                .Include(mq => mq.MetaQuestionTags)
                .ToDictionary(q => q.Id, q => q);

            var answerIds = questions.Where(q => q.Answers != null).SelectMany(q => q.Answers.Select(a => a.AnswerId)).Distinct().ToList();
            var answerLookup = context.MetaAnswers.Where(q => answerIds.Contains(q.Id)).ToDictionary(a => a.Id, a => a);

            var utcNow = _dateService.UtcNow;
            foreach (var metaQuestion in questions)
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

                var isBurnRequest = metaQuestion.Tags.Intersect(DbMetaTag.RequestTypes).Any();

                var trackedTags = dbMetaQuestion.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED).ToList();
                foreach (var tag in metaQuestion.Tags)
                {
                    if (!dbMetaQuestion.MetaQuestionMetaTags.Any(t => string.Equals(t.TagName, tag, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.MetaQuestionMetaTags.Add(new DbMetaQuestionMetaTag { TagName = tag, MetaQuestion = dbMetaQuestion });
                        if (tag == DbMetaTag.STATUS_FEATURED)
                        {
                            if (isBurnRequest)
                            {
                                dbMetaQuestion.FeaturedStarted = utcNow;
                                newFeatures.Add(new NewFeature { MetaQuestionId = metaQuestion.QuestionId.Value, MetaUrl = metaQuestion.Link, Tags = trackedTags.Select(t => t.TagName).ToList() });
                            }
                        }

                        if (tag == DbMetaTag.STATUS_PLANNED)
                        {
                            if (isBurnRequest)
                            {
                                dbMetaQuestion.BurnStarted = utcNow;
                                burnsStarted.Add(new BurnStarted {MetaUrl = metaQuestion.Link, Tags = trackedTags.Select(t => t.TagName).ToList()});
                            }
                        }
                    }
                }

                var metaQuestionList = dbMetaQuestion.MetaQuestionMetaTags.ToList();
                foreach (var dbTag in metaQuestionList)
                {
                    if (!metaQuestion.Tags.Any(t => string.Equals(t, dbTag.TagName, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.MetaQuestionMetaTags.Remove(dbTag);

                        if (dbTag.TagName == DbMetaTag.STATUS_FEATURED)
                        {
                            if (isBurnRequest)
                                dbMetaQuestion.FeaturedEnded = utcNow;
                        }

                        if (dbTag.TagName == DbMetaTag.STATUS_PLANNED)
                        {
                            if (isBurnRequest)
                            {
                                dbMetaQuestion.BurnEnded = utcNow;
                                foreach (var trackedTag in trackedTags)
                                    finishedBurns.Add(new BurnFinished {Tag = trackedTag.TagName});
                            }
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
            }

            var currentTags = context.MetaQuestionMetaTags.Local.Select(mqmt => mqmt.TagName).Distinct().ToList();
            var dbTags = context.MetaTags.Where(t => currentTags.Contains(t.Name)).ToLookup(t => t.Name);
            foreach (var currentTag in currentTags)
            {
                if (!dbTags.Contains(currentTag))
                    context.MetaTags.Add(new DbMetaTag { Name = currentTag });
            }

            context.SaveChanges();

            return new ProcessQuestionsResult
            {
                NewFeatures = newFeatures,
                BurnsStarted = burnsStarted,
                FinishedBurns = finishedBurns
            };
        }
    }
}
