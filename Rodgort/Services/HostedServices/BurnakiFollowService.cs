using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rodgort.Data;
using Rodgort.Data.Constants;
using Rodgort.Data.Tables;
using Rodgort.Utilities;
using Rodgort.Utilities.ReactiveX;
using StackExchangeChat;
using StackExchangeChat.Utilities;

#pragma warning disable 4014

namespace Rodgort.Services.HostedServices
{
    public class BurnakiFollowService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BurnakiFollowService> _logger;

        public BurnakiFollowService(IServiceProvider serviceProvider, ILogger<BurnakiFollowService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var configuration = _serviceProvider.GetRequiredService<IChatCredentials>();
            var hasCookies = !string.IsNullOrWhiteSpace(configuration.AcctCookie);
            var hasCredentials = !string.IsNullOrWhiteSpace(configuration.AcctCookie) && !string.IsNullOrWhiteSpace(configuration.Password);

            if (!hasCookies && !hasCredentials)
                return;

            try
            {
                var chatClient = _serviceProvider.GetRequiredService<ChatClient>();
                var dateService = _serviceProvider.GetRequiredService<DateService>();

                var context = _serviceProvider.GetRequiredService<RodgortContext>();

                var burnakiFollows = context.BurnakiFollows.Where(bf => !bf.FollowEnded.HasValue).ToList();
                foreach (var burnakiFollow in burnakiFollows)
                    FollowInRoom(burnakiFollow.RoomId, burnakiFollow.BurnakiId, burnakiFollow.FollowStarted,
                        burnakiFollow.Tag, dateService, cancellationToken);

                var events = chatClient.SubscribeToEvents(ChatSite.StackOverflow, ChatRooms.HEADQUARTERS);
                await events.FirstAsync();
                chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.HEADQUARTERS, "o/");
                _logger.LogInformation("Successfully joined headquarters");
                events
                    .ReplyAlive()
                    .Pinged()
                    .SameRoomOnly()
                    .Where(r => r.ChatEventDetails.UserId == ChatUserIds.ROB)
                    .Subscribe(
                        async chatEvent =>
                        {
                            await ParseCommands(chatClient, chatEvent, dateService, cancellationToken);
                        }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed chat");
            }
        }

        public async Task FollowInRoom(int roomId, int followingUserId, DateTime fromTime, string followingTag, DateService dateService, CancellationToken cancellationToken)
        {
            try
            {
                var chatClient = _serviceProvider.GetRequiredService<ChatClient>();

                var questionIdRegex = new Regex(@"stackoverflow\.com\/q\/(\d+)");
                
                var events = chatClient.SubscribeToEvents(ChatSite.StackOverflow, roomId);
                await events.FirstAsync();
                _logger.LogInformation($"Successfully joined room {roomId}");
                chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.HEADQUARTERS, $"I just joined {roomId}");

                events
                    .ReplyAlive()
                    .OnlyMessages()
                    .SameRoomOnly()
                    .Where(r => r.ChatEventDetails.UserId == followingUserId)
                    .SlidingBuffer(TimeSpan.FromSeconds(30))
                    .Subscribe(async chatEvents =>
                    {
                        try
                        {
                            var questionIds = chatEvents
                                .SelectMany(ceg => questionIdRegex.Matches(ceg.ChatEventDetails.Content).Select(m => int.Parse(m.Groups[1].Value)))
                                .Distinct();

                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var burnProcessingService = scope.ServiceProvider.GetRequiredService<BurnProcessingService>();
                                await burnProcessingService.ProcessQuestionIds(questionIds, followingTag, roomId, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Failed processing chat events", ex);
                        }
                    }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed chat");
            }
        }

        private async Task ParseCommands(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken)
        {
            var splitContent = chatEvent.ChatEventDetails.Content.Split(" ");
            if (splitContent.Length < 2)
                return;
            if (!string.Equals(splitContent[0], $"@{chatEvent.RoomDetails.MyUserName}"))
                return;

            var commandList = new Dictionary<string, ProcessCommand>
            {
                { "follow", ProcessFollow },
                { "unfollow", ProcessUnfollow },
                { "follows", ProcessFollows }
            };
            
            var command = splitContent[1];
            if (commandList.ContainsKey(command))
                await commandList[command](chatClient, chatEvent, dateService, cancellationToken, splitContent.Skip(2).ToList());
        }

        private delegate Task ProcessCommand(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args);

        private async Task ProcessFollow(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args)
        {
            if (args.Count != 3)
                return;
            if (!int.TryParse(args[0], out var burnakiUserId))
                return;
            if (!int.TryParse(args[1], out var roomId))
                return;

            var followingTag = args[2];
            if (string.IsNullOrWhiteSpace(followingTag))
                return;
            
            var innerContext = _serviceProvider.GetRequiredService<RodgortContext>();
            if (innerContext.BurnakiFollows.Any(bf => bf.BurnakiId == burnakiUserId && bf.RoomId == roomId && !bf.FollowEnded.HasValue))
            {
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} That follow is already registered!");
            }
            else
            {
                innerContext.BurnakiFollows.Add(new DbBurnakiFollow
                {
                    BurnakiId = burnakiUserId,
                    RoomId = roomId,
                    Tag = followingTag,
                    FollowStarted = dateService.UtcNow
                });
                innerContext.SaveChanges();

                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $"Okay, following {burnakiUserId} in {roomId}");

                FollowInRoom(roomId, burnakiUserId, DateTime.UtcNow, followingTag, dateService, cancellationToken);
            }
        }

        private async Task ProcessUnfollow(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args)
        {
            if (!int.TryParse(args[0], out var burnakiUserId))
                return;
            if (!int.TryParse(args[1], out var roomId))
                return;

            var innerContext = _serviceProvider.GetRequiredService<RodgortContext>();
            var existingFollow = innerContext.BurnakiFollows.FirstOrDefault(bf => bf.BurnakiId == burnakiUserId && bf.RoomId == roomId && !bf.FollowEnded.HasValue);
            if (existingFollow != null)
            {
                existingFollow.FollowEnded = dateService.UtcNow;
                innerContext.SaveChanges();
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $"Okay, unfollowing {burnakiUserId} in {roomId}");
            }
            else
            {
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} That follow doesn't exist");
            }
        }

        private async Task ProcessFollows(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args)
        {
            var innerContext = _serviceProvider.GetRequiredService<RodgortContext>();
            var allFollows = innerContext.BurnakiFollows.Where(bf => !bf.FollowEnded.HasValue);
            if (allFollows.Any())
            {
                var followMessage = $"I'm following: {string.Join(", ", allFollows.Select(f => $"{f.BurnakiId} in {f.RoomId} for tag {f.Tag}"))}";
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} {followMessage}");
            }
            else
            {
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} I'm not following anyone");
            }
        }
    }
}
