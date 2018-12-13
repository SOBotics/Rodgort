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
                        if (matchedTag.StatusId == DbMetaQuestionTagStatus.PENDING)
                            metaQuestionTag = matchedTag;
                        else
                            continue;
                    }
                    else
                    {
                        metaQuestionTag = new DbMetaQuestionTag();
                        isNew = true;
                    }

                    var metaTags = question.MetaQuestionMetaTags.ToLookup(t => t.TagName);
                    var requestType = DbRequestType.UNKNOWN;

                    var burnination = metaTags.Contains(DbMetaTag.BURNINATE_REQUEST);
                    var synonym = metaTags.Contains(DbMetaTag.SYNONYM_REQUEST);
                    var retag = metaTags.Contains(DbMetaTag.RETAG_REQUEST);

                    if (burnination && !synonym)
                        requestType = DbRequestType.BURNINATE;
                    else if (synonym && !burnination)
                        requestType = DbRequestType.SYNONYM;
                    else if (retag && !burnination)
                        requestType = DbRequestType.MERGE;

                    metaQuestionTag.MetaQuestion = question;
                    metaQuestionTag.TagName = matchedTagName;
                    metaQuestionTag.RequestTypeId = requestType;
                    metaQuestionTag.StatusId = DbMetaQuestionTagStatus.PENDING;

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
