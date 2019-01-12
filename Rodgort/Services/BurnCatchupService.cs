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
        public const string SERVICE_NAME = "Burn catchup";

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

        public async Task Catchup()
        {
            var currentBurnTags =
                _context.MetaQuestions
                    .Where(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_PLANNED))
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
            var result = await _apiClient.QuestionsBy("stackoverflow.com", tag);
            var idsToProcess = result.Items
                .Where(i => i.Tags.Contains(tag))
                .Select(i => i.QuestionId)
                .ToList();

            var burnProcessingService = _serviceProvider.GetRequiredService<BurnProcessingService>();
            await burnProcessingService.ProcessQuestionIds(idsToProcess, tag, null, null, false);

            var allIdsSeen = result.Items
                .Select(i => i.QuestionId)
                .ToList();

            var missingPostIds = _context
                .UserActions
                .Where(ua => ua.Tag == tag)
                .Where(ua => !allIdsSeen.Contains(ua.PostId))                
                .Where(g => !_context.UserActions.Any(ua => ua.PostId == g.PostId && ua.UserActionTypeId == DbUserActionType.DELETED))
                .Select(g => g.PostId)
                .Distinct()
                .ToList();

            foreach (var missingPostId in missingPostIds)
            {
                if (!_context.UserActions.Any(ua =>
                    ua.PostId == missingPostId
                    && ua.UserActionTypeId == DbUserActionType.UNKNOWN_DELETION
                    && ua.Tag == tag))
                {
                    _context.UserActions.Add(
                        new DbUserAction
                        {
                            UserActionTypeId = DbUserActionType.UNKNOWN_DELETION,
                            Tag = tag,
                            PostId = missingPostId,
                            Time = _dateService.UtcNow,
                            SiteUserId = -1
                        });
                }
            }
        }
    }
}
