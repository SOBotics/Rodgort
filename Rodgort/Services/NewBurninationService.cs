﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rodgort.Data;
using Rodgort.Data.Tables;
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

        public async Task AnnounceMultipleTrackedTags(string metaPostUrl, IEnumerable<string> trackedTags)
        {
            if (!_enabled)
                return;

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"Discussion for the burnination of {metaPostUrl} started, but there are multiple tracked tags: {string.Join(", ", trackedTags)}");
        }

        public async Task AnnounceNoTrackedTags(string metaPostUrl)
        {
            if (!_enabled)
                return;

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"Discussion for the burnination of {metaPostUrl} started, but there are no tracked tags");
        }

        public async Task StopBurn(string tag)
        {
            if (!_enabled)
                return;

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, $"@Gemmy stop tag {tag}");
            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, $"The burnination of [tag:{tag}] has finished!");

            var follows = _rodgortContext.BurnakiFollows.Where(bf => bf.BurnakiId == 8300708 && bf.Tag == tag).ToList();
            foreach (var follow in follows)
            {
                follow.FollowEnded = _dateService.UtcNow;
            }

            _rodgortContext.SaveChanges();
        }
        
        public async Task CreateRoomForBurn(string tag, string metaPostUrl)
        {
            if (!_enabled)
                return;

            var roomName = $"Observation room for [{tag}] burnination";
            var roomId = await _chatClient.CreateRoom(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, roomName, string.Empty);

            var gemmyMessage = $"@Gemmy start tag [{tag}] {roomId} https://chat.stackoverflow.com/rooms/{roomId}";

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, gemmyMessage);

            var burninationMessage = $"The burnination of [tag:{tag}] is now being discussed {metaPostUrl}";

            var burninationMessageId = await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.TROGDOR, burninationMessage);
            await _chatClient.PinMessage(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, burninationMessageId);

            var burnakiFollow = new DbBurnakiFollow
            {
                BurnakiId = 8300708,
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
    }
}
