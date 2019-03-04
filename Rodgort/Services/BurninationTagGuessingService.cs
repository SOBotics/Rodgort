using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rodgort.Data;
using Rodgort.Data.Tables;

namespace Rodgort.Services
{
    public class BurninationTagGuessingService
    {
        private readonly RodgortContext _context;
        private readonly DateService _dateService;
        private readonly ILogger<BurninationTagGuessingService> _logger;

        public BurninationTagGuessingService(
            RodgortContext context, 
            DateService dateService,
            ILogger<BurninationTagGuessingService> logger)
        {
            _context = context;
            _dateService = dateService;
            _logger = logger;
        }

        private static readonly Regex _tagNameMatcher = new Regex("\\[tag:(?<tagName>([^\\]]+))\\]");
        public void GuessTags(IEnumerable<int> questionIds)
        {
            var questions = _context.MetaQuestions
                .Where(q => questionIds.Contains(q.Id))
                .Include(mq => mq.MetaQuestionMetaTags)
                .Include(mq => mq.MetaQuestionTags)
                .ToList();

            _logger.LogInformation($"Guessing tags for {questions.Count} questions");

            foreach (var question in questions)
            {
                var matches = _tagNameMatcher.Matches(question.Body);
                var matchedTagNames = matches.Select(m => m.Groups["tagName"].Value)
                    .Select(t => t.ToLower())
                    .Distinct()
                    .ToList();

                var existingTags = question.MetaQuestionTags.ToDictionary(d => d.TagName, d => d, StringComparer.OrdinalIgnoreCase);

                foreach (var matchedTagName in matchedTagNames)
                {
                    DbMetaQuestionTag metaQuestionTag;
                    var isNew = false;
                    if (existingTags.ContainsKey(matchedTagName))
                    {
                        var matchedTag = existingTags[matchedTagName];
                        if (matchedTag.TrackingStatusId == DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL 
                            || matchedTag.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE)
                            metaQuestionTag = matchedTag;
                        else
                            continue;
                    }
                    else
                    {
                        metaQuestionTag = new DbMetaQuestionTag();
                        isNew = true;
                    }

                    metaQuestionTag.MetaQuestion = question;
                    metaQuestionTag.TagName = matchedTagName;
                    
                    // If there's only one tag, and that tag is found in the title in the form of [tag], we can just track it.
                    if (matchedTagNames.Count == 1 && question.Title.Contains($"[{matchedTagName}]"))
                        ChangeTrackingStatusId(metaQuestionTag, DbMetaQuestionTagTrackingStatus.TRACKED);
                    else
                    {
                        var trackedElsewhere = _context.MetaQuestionTags.Any(t => 
                            t.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                            && t.TagName == matchedTagName);
                        
                        // If we find a tag marked 'tracked elsewhere', but can't find any other tracked tags, put it back to requires approval
                        if (metaQuestionTag.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE && !trackedElsewhere)
                            ChangeTrackingStatusId(metaQuestionTag, DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL);
                        // Otherwise, if it's tracked elsewhere, mark it as such
                        else if (trackedElsewhere)
                            ChangeTrackingStatusId(metaQuestionTag, DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE);
                        else
                            ChangeTrackingStatusId(metaQuestionTag, DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL);
                    }
                        
                    
                    if (isNew)
                        _context.MetaQuestionTags.Add(metaQuestionTag); 
                }
            }

            var currentTags = _context.MetaQuestionTags.Local.Select(mqmt => mqmt.TagName).Distinct().ToList();
            var dbTags = _context.Tags.Where(t => currentTags.Contains(t.Name)).ToLookup(t => t.Name);
            foreach (var currentTag in currentTags)
            {
                if (!dbTags.Contains(currentTag))
                    _context.Tags.Add(new DbTag { Name = currentTag });
            }

            _context.SaveChanges();

            var masterSynonymTags =
                _context.MetaQuestionTags
                    .Where(mqt => questionIds.Contains(mqt.MetaQuestionId))
                    .Where(mqt =>
                        (
                            mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL
                            || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE
                        )
                        && _context.MetaQuestionTags.Any(innerMqt =>
                            innerMqt.MetaQuestionId == mqt.MetaQuestionId
                            && innerMqt.Tag.SynonymOf != null
                            && innerMqt.Tag.SynonymOf.Name == mqt.TagName
                        )
                    )
                    .ToList();

            var childSynonymTags = _context.MetaQuestionTags.Where(mqt =>
                (
                    mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL 
                    || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE
                )
                && mqt.Tag.SynonymOf != null
            ).ToList();

            foreach (var masterSynonymTag in masterSynonymTags)
                ChangeTrackingStatusId(masterSynonymTag, DbMetaQuestionTagTrackingStatus.IGNORED);

            foreach (var childSynonymTag in childSynonymTags)
                ChangeTrackingStatusId(childSynonymTag, DbMetaQuestionTagTrackingStatus.TRACKED);

            if (masterSynonymTags.Any() || childSynonymTags.Any())
            {
                _logger.LogInformation($"Automatically ignoring {masterSynonymTags.Count} as synonym masters in a question and tracking {childSynonymTags.Count} synonym children");
                _context.SaveChanges();
            }

            _logger.LogTrace("Guessing tags completed");
        }

        private void ChangeTrackingStatusId(DbMetaQuestionTag questionTag, int newStatus)
        {
            if (questionTag.TrackingStatusId == newStatus)
                return;

            _context.MetaQuestionTagTrackingStatusAudits.Add(new DbMetaQuestionTagTrackingStatusAudit
            {
                ChangedByUserId = -1,
                Tag = questionTag.TagName,
                MetaQuestionId = questionTag.MetaQuestionId,
                PreviousTrackingStatusId = questionTag.TrackingStatusId,
                NewTrackingStatusId = newStatus,
                TimeChanged = _dateService.UtcNow
            });
            questionTag.TrackingStatusId = newStatus;
        }
    }
}
