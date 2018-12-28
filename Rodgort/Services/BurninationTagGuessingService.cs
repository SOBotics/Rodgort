using System;
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
        public const string SERVICE_NAME = "Guess burnination tags";

        private readonly RodgortContext _context;
        private readonly ILogger<BurninationTagGuessingService> _logger;

        public BurninationTagGuessingService(RodgortContext context, ILogger<BurninationTagGuessingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private static readonly Regex _tagNameMatcher = new Regex("\\[tag:(?<tagName>([^\\]]+))\\]");
        public void GuessTags()
        {
            var questions = _context.MetaQuestions
                .Include(mq => mq.MetaQuestionMetaTags)
                .Include(mq => mq.MetaQuestionTags)
                .ToList();

            _logger.LogInformation($"Starting to guess tags for {questions.Count} questions");

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
                        metaQuestionTag.TrackingStatusId = DbMetaQuestionTagTrackingStatus.TRACKED;
                    else
                    {
                        var trackedElsewhere = _context.MetaQuestionTags.Any(t => 
                            t.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                            && t.TagName == matchedTagName);
                        
                        // If we find a tag marked 'tracked elsewhere', but can't find any other tracked tags, put it back to requires approval
                        if (metaQuestionTag.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE && !trackedElsewhere)
                            metaQuestionTag.TrackingStatusId = DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL;
                        // Otherwise, if it's tracked elsewhere, mark it as such
                        else if (trackedElsewhere)
                            metaQuestionTag.TrackingStatusId = DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE;
                        else
                            metaQuestionTag.TrackingStatusId = DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL;
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

            _logger.LogInformation("Guessing tags completed");
        }
    }
}
