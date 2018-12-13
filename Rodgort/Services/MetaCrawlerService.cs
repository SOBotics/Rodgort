﻿using System;
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
using StackExchangeApi;
using StackExchangeApi.Responses;

namespace Rodgort.Services
{
    public class MetaCrawlerService
    {
        public const string SERVICE_NAME = "Refresh burnination request list";

        private readonly RodgortContext _context;
        private readonly ApiClient _apiClient;
        private readonly DateService _dateService;
        private readonly ILogger<MetaCrawlerService> _logger;

        private static readonly object _locker = new object();
        private static bool _alreadyProcessing;

        public MetaCrawlerService(RodgortContext context, ApiClient apiClient, DateService dateService, ILogger<MetaCrawlerService> logger)
        {
            _context = context;
            _apiClient = apiClient;
            _dateService = dateService;
            _logger = logger;
        }

        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField", Justification = "Flagged _logger, but in both blocks we're guaranteed to run on a single thread")]
        public async Task CrawlMeta()
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
                _logger.LogInformation("Starting meta crawl");
                var metaQuestions = await _apiClient.MetaQuestionsByTag("meta.stackoverflow.com", "burninate-request");

                _logger.LogInformation($"{metaQuestions.Items.Count} meta questions retrieved. Processing...");

                var questionIds = metaQuestions.Items.Select(q => q.QuestionId).Distinct().ToList();
                var questionLookup = _context.MetaQuestions.Where(q => questionIds.Contains(q.Id))
                    .Include(mq => mq.MetaQuestionMetaTags)
                    .ToDictionary(q => q.Id, q => q);

                var answerIds = metaQuestions.Items.Where(q => q.Answers != null).SelectMany(q => q.Answers.Select(a => a.AnswerId)).Distinct().ToList();
                var answerLookup = _context.MetaAnswers.Where(q => answerIds.Contains(q.Id))
                    .ToDictionary(a => a.Id, a => a);

                var utcNow = _dateService.UtcNow;
                foreach (var metaQuestion in metaQuestions.Items)
                {
                    DbMetaQuestion dbMetaQuestion;
                    if (!metaQuestion.QuestionId.HasValue)
                        throw new InvalidOperationException($"Question object does not contain {nameof(metaQuestion.QuestionId)}");

                    if (!questionLookup.ContainsKey(metaQuestion.QuestionId.Value))
                    {
                        dbMetaQuestion = new DbMetaQuestion {Id = metaQuestion.QuestionId.Value};
                        _context.MetaQuestions.Add(dbMetaQuestion);

                        questionLookup[dbMetaQuestion.Id] = dbMetaQuestion;
                    }
                    else
                    {
                        dbMetaQuestion = questionLookup[metaQuestion.QuestionId.Value];
                    }

                    dbMetaQuestion.Title = WebUtility.HtmlDecode(metaQuestion.Title);
                    dbMetaQuestion.Body = WebUtility.HtmlDecode(metaQuestion.BodyMarkdown);
                    dbMetaQuestion.Link = metaQuestion.Link;
                    dbMetaQuestion.LastSeen = utcNow;

                    foreach (var tag in metaQuestion.Tags)
                        if (!dbMetaQuestion.MetaQuestionMetaTags.Any(t => string.Equals(t.TagName, tag, StringComparison.OrdinalIgnoreCase)))
                            _context.MetaQuestionMetaTags.Add(new DbMetaQuestionMetaTag { TagName = tag, MetaQuestion = dbMetaQuestion });

                    foreach(var dbTag in dbMetaQuestion.MetaQuestionMetaTags.ToList())
                        if (!metaQuestion.Tags.Any(t => string.Equals(t, dbTag.TagName, StringComparison.OrdinalIgnoreCase)))
                            _context.MetaQuestionMetaTags.Remove(dbTag);

                    if (!metaQuestion.Score.HasValue)
                        throw new InvalidOperationException($"Question object does not contain {nameof(metaQuestion.Score)}");

                    var dbMetaQuestionStatistics = new DbMetaQuestionStatistics
                    {
                        DateTime = utcNow,
                        Score = metaQuestion.Score.Value,

                        MetaQuestion = dbMetaQuestion
                    };
                    _context.Add(dbMetaQuestionStatistics);

                    foreach (var metaAnswer in metaQuestion.Answers ?? Enumerable.Empty<BaseAnswer>())
                    {
                        DbMetaAnswer dbMetaAnswer;
                        if (!metaAnswer.AnswerId.HasValue)
                            throw new InvalidOperationException($"Answer object does not contain {nameof(metaAnswer.AnswerId)}");

                        if (!answerLookup.ContainsKey(metaAnswer.AnswerId.Value))
                        {
                            dbMetaAnswer = new DbMetaAnswer {Id = metaAnswer.AnswerId.Value};
                            _context.MetaAnswers.Add(dbMetaAnswer);

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

                        _context.Add(dbMetaAnswerStatistics);
                    }
                }

                var currentTags = _context.MetaQuestionMetaTags.Local.Select(mqmt => mqmt.TagName).Distinct().ToList();
                var dbTags = _context.MetaTags.Where(t => currentTags.Contains(t.Name)).ToLookup(t => t.Name);
                foreach (var currentTag in currentTags)
                {
                    if (!dbTags.Contains(currentTag))
                        _context.MetaTags.Add(new DbMetaTag {Name = currentTag});
                }
                
                _context.SaveChanges();

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
