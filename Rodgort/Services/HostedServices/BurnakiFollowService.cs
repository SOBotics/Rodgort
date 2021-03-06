﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Utilities;
using Rodgort.Utilities.ReactiveX;
using StackExchangeApi;
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
            _serviceProvider = serviceProvider.CreateScope().ServiceProvider;
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
                    FollowInRoom(burnakiFollow.RoomId, burnakiFollow.BurnakiId, burnakiFollow.FollowStarted, burnakiFollow.Tag, dateService, cancellationToken);

                var headquarterEvents = chatClient.SubscribeToEvents(ChatSite.StackOverflow, ChatRooms.HEADQUARTERS);
                await headquarterEvents.FirstAsync();

                chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.HEADQUARTERS, "o/");
                _logger.LogInformation("Successfully joined headquarters");

                var trogdorEvents = chatClient.SubscribeToEvents(ChatSite.StackOverflow, ChatRooms.TROGDOR);
                await trogdorEvents.FirstAsync();
                _logger.LogInformation("Successfully joined trogdor");

                headquarterEvents
                    .Merge(trogdorEvents)
                    .ReplyAlive()
                    .Pinged()
                    .SameRoomOnly()
                    .Where(e =>
                    {
                        var innerContext = _serviceProvider.GetRequiredService<RodgortContext>();
                        var isTriager = innerContext.SiteUsers.Any(su => su.Id == e.ChatEventDetails.UserId && su.Roles.Any(r => r.RoleId == DbRole.TRIAGER));
                        return isTriager;
                    })
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

                try
                {
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
                                _logger.LogError(ex, "Failed processing chat events");
                            }
                        }, cancellationToken);
                }
                catch (SiteAuthenticator.RoomNotFoundException)
                {
                    throw;
                    //using (var context = _serviceProvider.GetRequiredService<RodgortContext>())
                    //{
                    //    var followsForRoom = context.BurnakiFollows.Where(bf => bf.RoomId == roomId).ToList();
                    //    if (followsForRoom.Any())
                    //    {
                    //        throw;
                    //    }
                    //}
                }
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

            var helpList = new[]
            {
                "tracking	- List of burninations Rodgort has instructed Gemmy to watch and haven't yet been untracked",
                "untrack {tags}	- Instructs Rodgort to stop following the tags (space separated), and to instruct Gemmy to stop watching the tags",
                "lastupdated	- Gets the last updated time",
                "update  	- Instructs Rodgort to update the statistics of the current burn",
                "quota  		- API quota remaining",
            };

            var commandList = new Dictionary<string, ProcessCommand>
            {
                { "tracking", ProcessTracking },
                { "untrack", ProcessUntrack },
                { "lastupdated", ProcessLastUpdated },
                { "update", ProcessUpdate },
                { "quota", ProcessQuota },
                { "help", async (_, __, ___, ____, _____) =>
                    {
                        await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, string.Join(Environment.NewLine, helpList.Select(s => "    " + s)));
                    }
                },
            };
            
            var command = splitContent[1];
            if (commandList.ContainsKey(command))
                await commandList[command](chatClient, chatEvent, dateService, cancellationToken, splitContent.Skip(2).ToList());
        }

        private delegate Task ProcessCommand(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args);
        
        private async Task ProcessTracking(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args)
        {
            var innerContext = _serviceProvider.GetRequiredService<RodgortContext>();
            var allFollows = innerContext.BurnakiFollows.Where(bf => !bf.FollowEnded.HasValue).ToList();
            if (allFollows.Any())
            {
                var trackingMessage = $"The following tags are being tracked: {string.Join(", ", allFollows.Select(f => f.Tag).Distinct())}";
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} {trackingMessage}");
            }
            else
            {
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} I'm not following anyone");
            }
        }

        private async Task ProcessUntrack(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args)
        {
            var innerContext = _serviceProvider.GetRequiredService<RodgortContext>();
            var untrackedTags = new List<string>();
            foreach (var tag in args)
            {
                var follows = innerContext.BurnakiFollows.Where(bf => !bf.FollowEnded.HasValue && bf.Tag == tag).ToList();
                if (follows.Any())
                {
                    await chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, $"@Gemmy stop tag {tag}");
                    foreach (var follow in follows)
                    {
                        follow.FollowEnded = dateService.UtcNow;
                        await chatClient.SendMessage(ChatSite.StackOverflow, follow.RoomId, "@Gemmy stop");
                        untrackedTags.Add(tag);
                    }
                }
                else
                {
                    await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} {tag} is not being tracked");
                }
            }

            innerContext.SaveChanges();
            if (untrackedTags.Any())
            {
                var trackingMessage = $"Tag(s) {string.Join(", ", untrackedTags)} successfully untracked";
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} {trackingMessage}");
            }
        }

        private async Task ProcessLastUpdated(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var job = connection.GetRecurringJobs().First(r => r.Id == BurnCatchupService.SERVICE_NAME);
                if (!job.LastExecution.HasValue)
                    await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} No last update time found");
                else
                    await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} Last updated: {job.LastExecution.Value:yyyy-MM-dd hh:mm:ss} ({Math.Round((DateTime.UtcNow - job.LastExecution.Value).TotalHours, 1)} hours ago)");
            }
        }

        private static async Task ProcessUpdate(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args)
        {
            RecurringJob.Trigger(BurnCatchupService.SERVICE_NAME);

            await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} triggered");
        }
        
        private static async Task ProcessQuota(ChatClient chatClient, ChatEvent chatEvent, DateService dateService, CancellationToken cancellationToken, List<string> args)
        {
            var quotaRemaining = ApiClient.CurrentQuotaRemaining;
            if (quotaRemaining == int.MaxValue)
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} Unknown, Rodgort must have recently rebooted. Try again in a few minutes.");
            else
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} {quotaRemaining:n0}/10,000 remaining.");
        }
    }
}
