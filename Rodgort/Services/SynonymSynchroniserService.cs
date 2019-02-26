using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rodgort.Data;
using Rodgort.Data.Tables;
using StackExchangeApi;

namespace Rodgort.Services
{
    public class SynonymSynchroniserService
    {
        private readonly RodgortContext _context;
        private readonly ApiClient _apiClient;
        private readonly DateService _dateService;
        private readonly ILogger<SynonymSynchroniserService> _logger;

        public const string SERVICE_NAME = "Synonym synchroniser service";
        
        public SynonymSynchroniserService(
            RodgortContext context,
            ApiClient apiClient,
            DateService dateService,
            ILogger<SynonymSynchroniserService> logger)
        {
            _context = context;
            _apiClient = apiClient;
            _dateService = dateService;
            _logger = logger;
        }

        public void SynchroniseSync()
        {
            Synchronise().Wait();
        }

        private async Task Synchronise()
        {
            _logger.LogInformation("Beginning to synchronise synonyms");

            var response = await _apiClient.TagSynonyms("stackoverflow.com");
            var tagLookup = response.Items.ToDictionary(i => i.FromTag);
            
            var existingTags = _context.Tags.ToList();
            foreach (var existingTag in existingTags)
            {
                if (tagLookup.ContainsKey(existingTag.Name))
                {
                    var synonymOf = tagLookup[existingTag.Name].ToTag;
                    if (existingTag.SynonymOfTagName != synonymOf)
                    {
                        existingTag.SynonymOfTagName = synonymOf;
                        existingTag.NumberOfQuestions = 0;

                        _context.TagStatistics.Add(new DbTagStatistics
                        {
                            QuestionCount = 0,
                            DateTime = _dateService.UtcNow,
                            TagName = existingTag.Name,
                            IsSynonym = true
                        });
                    }
                }
                else
                    existingTag.SynonymOfTagName = null;
            }
            
            var currentTags = existingTags.Select(mqmt => mqmt.SynonymOfTagName).Where(n => n != null).Distinct().ToList();
            var dbTags = _context.Tags.Where(t => currentTags.Contains(t.Name)).ToLookup(t => t.Name);
            foreach (var currentTag in currentTags)
            {
                if (!dbTags.Contains(currentTag))
                    _context.Tags.Add(new DbTag { Name = currentTag });
            }
            
            _context.SaveChanges();

            _logger.LogInformation("Finished synchronising synonyms");
        }
    }
}
