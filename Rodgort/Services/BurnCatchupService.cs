using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rodgort.Data;
using Rodgort.Data.Tables;
using StackExchangeApi;

namespace Rodgort.Services
{
    public class BurnCatchupService
    {
        public const string SERVICE_NAME = "Burn catchup no announce";

        private readonly ApiClient _apiClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly DateService _dateService;
        private readonly RodgortContext _context;

        public BurnCatchupService(
            ApiClient apiClient, 
            IServiceProvider serviceProvider,
            DateService dateService,
            RodgortContext context)
        {
            _apiClient = apiClient;
            _serviceProvider = serviceProvider;
            _dateService = dateService;
            _context = context;
        }

        public void CatchupSync()
        {
            Catchup().Wait();
        }

        private async Task Catchup()
        {
            var currentBurnTags =
                _context.MetaQuestions
                    .Where(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_PLANNED || mqmt.TagName == DbMetaTag.STATUS_FEATURED))
                    .SelectMany(
                        mq => mq.MetaQuestionTags.Where(mqt =>
                                mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                                || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE)
                            .Select(mqt => mqt.TagName)
                    )
                    .Distinct()
                    .ToList();

            foreach (var currentBurnTag in currentBurnTags)
                await Catchup(currentBurnTag);
        }

        private async Task Catchup(string tag)
        {
            var result = await _apiClient.QuestionsByTag("stackoverflow.com", tag);
            var idsToProcess = result.Items
                .Where(i => i.Tags.Contains(tag))
                .Select(i => i.QuestionId)
                .ToList();

            var seenQuestionsNotDeleted = _context
                .UserActions
                .Where(ua => ua.Tag == tag)
                .Where(g => !_context.UserActions.Any(ua => ua.PostId == g.PostId && ua.UserActionTypeId == DbUserActionType.DELETED))
                .Select(g => g.PostId)
                .Distinct()
                .ToList();

            var burnProcessingService = _serviceProvider.GetRequiredService<BurnProcessingService>();
            await burnProcessingService.ProcessQuestionIds(idsToProcess.Concat(seenQuestionsNotDeleted), tag, null, false);            
        }
    }
}
