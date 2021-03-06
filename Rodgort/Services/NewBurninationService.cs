﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Rodgort.Data;
using Rodgort.Data.Constants;
using Rodgort.Data.Tables;
using Rodgort.Services.HostedServices;
using Rodgort.Utilities;
using StackExchangeChat;

namespace Rodgort.Services
{
    public class NewBurninationService
    {
        private readonly ChatClient _chatClient;
        private readonly RodgortContext _rodgortContext;
        private readonly DateService _dateService;
        private readonly BurnakiFollowService _burnakiFollowService;

        private readonly bool _enabled;

        public NewBurninationService(ChatClient chatClient, RodgortContext rodgortContext, DateService dateService, BurnakiFollowService burnakiFollowService, IChatCredentials chatCredentials)
        {
            _chatClient = chatClient;
            _rodgortContext = rodgortContext;
            _dateService = dateService;
            _burnakiFollowService = burnakiFollowService;

            var hasCookies = !string.IsNullOrWhiteSpace(chatCredentials.AcctCookie);
            var hasCredentials = !string.IsNullOrWhiteSpace(chatCredentials.AcctCookie) && !string.IsNullOrWhiteSpace(chatCredentials.Password);

            _enabled = hasCookies || hasCredentials;
        }

        private async Task AnnounceFeaturedMultipleTrackedTags(string metaPostUrl, IEnumerable<string> trackedTags)
        {
            if (!_enabled)
                return;

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"Discussion for the burnination of {metaPostUrl} started, but there are multiple tracked tags: {string.Join(", ", trackedTags)}");
        }

        private async Task AnnounceFeaturedNoTrackedTags(string metaPostUrl)
        {
            if (!_enabled)
                return;

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"Discussion for the burnination of {metaPostUrl} started, but there are no tracked tags");
        }

        private async Task AnnounceBurningMultipleTrackedTags(string metaPostUrl, IEnumerable<string> trackedTags)
        {
            if (!_enabled)
                return;

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"The burn of {metaPostUrl} started, but there are multiple tracked tags: {string.Join(", ", trackedTags)}");
        }

        private async Task AnnounceBurningNoTrackedTags(string metaPostUrl)
        {
            if (!_enabled)
                return;

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"The burn of {metaPostUrl} started, but there are no tracked tags");
        }

        private async Task AnnounceBurningMultipleRooms(string metaPostUrl, List<int> roomIds)
        {
            if (!_enabled)
                return;

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"The burn of {metaPostUrl} started, but there are multiple observation rooms: {string.Join(", ", roomIds)}");
        }
        
        public async Task AnnounceBurnEnded(string metaQuestionName, int metaQuestionId, IEnumerable<string> tags)
        {
            if (!_enabled)
                return;

            var tagsList = tags.ToList();
            metaQuestionName = metaQuestionName.Replace("[", "\\[").Replace("]", "\\]");
            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"The burnination of [{metaQuestionName}](https://meta.stackoverflow.com/q/{metaQuestionId}) {string.Join(", ", tagsList.Select(t => $"[tag:{t}]"))} has finished!");
            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"Room owners: you'll need to manually cleanup the burn rooms and gemmy by running `@Rodgort untrack {string.Join(" ", tagsList)}`");
        }

        public async Task AnnounceBurnDeclined(string metaQuestionName, int metaQuestionId, IEnumerable<string> tags)
        {
            if (!_enabled)
                return;

            var tagsList = tags.ToList();
            metaQuestionName = metaQuestionName.Replace("[", "\\[").Replace("]", "\\]");
            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"The request for the burnination [{metaQuestionName}](https://meta.stackoverflow.com/q/{metaQuestionId}) {string.Join(", ", tagsList.Select(t => $"[tag:{t}]"))} has been declined");
            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"Room owners: you'll need to manually cleanup the burn rooms and gemmy by running `@Rodgort untrack {string.Join(" ", tagsList)}`");
        }

        public async Task NewBurnStarted(string metaPostUrl, List<string> tags)
        {
            if (!_enabled)
                return;

            if (!tags.Any())
            {
                await AnnounceBurningNoTrackedTags(metaPostUrl);
                return;
            }

            if (tags.Count > 1)
            {
                await AnnounceBurningMultipleTrackedTags(metaPostUrl, tags);
                return;
            }

            var tag = tags.First();

            var observationRooms = 
                _rodgortContext.BurnakiFollows
                .Where(b => b.BurnakiId == ChatUserIds.GEMMY && !b.FollowEnded.HasValue && b.Tag == tag)
                .Select(bf => bf.RoomId)
                .Distinct()
                .ToList();

            if (observationRooms.Count > 1)
            {
                await AnnounceBurningMultipleRooms(metaPostUrl, observationRooms);
                return;
            }

            var roomId = observationRooms.FirstOrDefault();
            if (roomId == 0)
                roomId = await CreateBurnRoom(metaPostUrl, tag);
            else
                await RenameObservationRoom(roomId, metaPostUrl, tag);
            
            var closeQueueLink = QueryHelpers.AddQueryString("https://stackoverflow.com/review/close/", new Dictionary<string, string> { { "filter-tags", tag } });
            var openQuestionsLink = QueryHelpers.AddQueryString("https://stackoverflow.com/search", new Dictionary<string, string> { { "q", $"[{tag}] is:q closed:no" } });

            var burninationMessage = $"The burnination of [tag:{tag}] has STARTED! [Close Queue]({closeQueueLink}) - [Open questions]({openQuestionsLink}) - [Meta post]({metaPostUrl}) - [Burn room](https://chat.stackoverflow.com/rooms/{roomId}).";
            var messageId = await _chatClient.SendMessageAndPin(ChatSite.StackOverflow, ChatRooms.TROGDOR, burninationMessage);
            await UnpinMine(ChatSite.StackOverflow, ChatRooms.TROGDOR);
            SavePinned(ChatSite.StackOverflow, ChatRooms.TROGDOR, messageId);
        }

        private async Task RenameObservationRoom(int roomId, string metaPostUrl, string tag)
        {
            await _chatClient.EditRoom(ChatSite.StackOverflow, roomId, $"Burnination progress for the [{tag}] tag", metaPostUrl, new[] { tag });
        }

        private async Task<int> CreateBurnRoom(string metaPostUrl, string tag)
        {
            var roomName = $"Burnination progress for the [{tag}] tag";
            var roomId = await _chatClient.CreateRoom(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, roomName, metaPostUrl, new[] { tag });

            await _chatClient.AddWriteAccess(ChatSite.StackOverflow, roomId, ChatUserIds.GEMMY);

            var closeQueueLink = QueryHelpers.AddQueryString("https://stackoverflow.com/review/close/", new Dictionary<string, string> { { "filter-tags", tag } });
            var openQuestionsLink = QueryHelpers.AddQueryString("https://stackoverflow.com/search", new Dictionary<string, string> { { "q", $"[{tag}] is:q closed:no" } });

            var burninationMessage = $"[Close Queue]({closeQueueLink}) - [Open questions]({openQuestionsLink}) - [Meta post]({metaPostUrl}) - [Burn room](https://chat.stackoverflow.com/rooms/{roomId}).";
            await _chatClient.SendMessageAndPin(ChatSite.StackOverflow, roomId, burninationMessage);

            var gemmyMessage = $"@Gemmy start tag [{tag}] {roomId} {metaPostUrl}";

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, gemmyMessage);
            return roomId;
        }

        public async Task NewTagsFeatured(int metaQuestionId, string metaPostUrl, List<string> tags)
        {
            if (!_enabled)
                return;
            
            if (!tags.Any())
            {
                await AnnounceFeaturedNoTrackedTags(metaPostUrl);
                return;
            }

            if (tags.Count > 1)
            {
                await AnnounceFeaturedMultipleTrackedTags(metaPostUrl, tags);
                return;
            }

            var tag = tags.First();

            var follows = _rodgortContext.BurnakiFollows.Where(bf =>
                bf.BurnakiId == ChatUserIds.GEMMY
                && bf.Tag == tag
                && !bf.FollowEnded.HasValue
            ).ToList();

            if (follows.Any())
                return;

            var roomName = $"Observation room for [{tag}] burnination";
            var roomId = await _chatClient.CreateRoom(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, roomName, metaPostUrl, new[] { tag });
            await _chatClient.AddWriteAccess(ChatSite.StackOverflow, roomId, ChatUserIds.GEMMY);

            var closeQueueLink = QueryHelpers.AddQueryString("https://stackoverflow.com/review/close/", new Dictionary<string, string> { { "filter-tags", tag } });
            var openQuestionsLink = QueryHelpers.AddQueryString("https://stackoverflow.com/search", new Dictionary<string, string> { { "q", $"[{tag}] is:q closed:no" } });

            await _chatClient.SendMessageAndPin(ChatSite.StackOverflow, roomId,
                $"[Rodgort tag progress](https://rodgort.sobotics.org/progress?metaQuestionId={metaQuestionId}) - [Close Queue]({closeQueueLink}) - [Open questions]({openQuestionsLink}) - [Meta post]({metaPostUrl})");
            
            var gemmyMessage = $"@Gemmy start tag [{tag}] {roomId} {metaPostUrl}";
            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, gemmyMessage);

            var burninationMessage = $"The burnination of [tag:{tag}] is now being discussed. [Meta post]({metaPostUrl}). [Observation room](https://chat.stackoverflow.com/rooms/{roomId}).";

            var messageId = await _chatClient.SendMessageAndPin(ChatSite.StackOverflow, ChatRooms.TROGDOR, burninationMessage);
            await UnpinMine(ChatSite.StackOverflow, ChatRooms.TROGDOR);
            SavePinned(ChatSite.StackOverflow, ChatRooms.TROGDOR, messageId);

            var burnakiFollow = new DbBurnakiFollow
            {
                BurnakiId = ChatUserIds.GEMMY,
                FollowStarted = _dateService.UtcNow,
                RoomId = roomId,
                Tag = tag
            };

            _rodgortContext.BurnakiFollows.Add(burnakiFollow);
            _rodgortContext.SaveChanges();

            await _burnakiFollowService.FollowInRoom(
                burnakiFollow.RoomId, 
                burnakiFollow.BurnakiId, 
                burnakiFollow.FollowStarted, 
                burnakiFollow.Tag, 
                _dateService, 
                CancellationToken.None
            );
        }

        private void SavePinned(ChatSite chatSite, int roomId, int messageId)
        {
            _rodgortContext.PinnedMessages.Add(new DbPinnedMessages { MessageId = messageId, ChatDomain = chatSite.ChatDomain, RoomId = roomId });
            _rodgortContext.SaveChanges();
        }

        private async Task UnpinMine(ChatSite chatSite, int roomId)
        {
            var pinnedMessages = _rodgortContext.PinnedMessages.Where(pm => pm.ChatDomain == chatSite.ChatDomain && pm.RoomId == roomId).ToList();
            await _chatClient.UnpinMessages(chatSite, roomId, pinnedMessages.Select(pm => pm.MessageId).ToArray());
            foreach (var pinnedMessage in pinnedMessages)
                _rodgortContext.PinnedMessages.Remove(pinnedMessage);
            
            _rodgortContext.SaveChanges();
        }
    }
}
