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
using MoreLinq;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Utilities;
using Rodgort.Utilities.ReactiveX;
using StackExchangeApi;
using StackExchangeChat;
using StackExchangeChat.Utilities;
#pragma warning disable 4014

namespace Rodgort.Services
{
    public class BurnakiFollowService : IHostedService
    {
        private const int Headquarters = 185585;
        private const int RobUserId = 563532;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BurnakiFollowService> _logger;

        public BurnakiFollowService(IServiceProvider serviceProvider, ILogger<BurnakiFollowService> logger)
        {
            _serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await RunWithLogging(async () =>
            {
                var chatClient = _serviceProvider.GetRequiredService<ChatClient>();
                var dateService = _serviceProvider.GetRequiredService<DateService>();

                var context = _serviceProvider.GetRequiredService<RodgortContext>();

                var burnakiFollows = context.BurnakiFollows.Where(bf => !bf.FollowEnded.HasValue).ToList();
                foreach (var burnakiFollow in burnakiFollows)
                    FollowInRoom(burnakiFollow.RoomId, burnakiFollow.BurnakiId, burnakiFollow.FollowStarted, cancellationToken);

                var events = chatClient.SubscribeToEvents(ChatSite.StackOverflow, Headquarters);
                await events.FirstAsync();
                chatClient.SendMessage(ChatSite.StackOverflow, Headquarters, "o/");
                _logger.LogInformation("Successfully joined headquarters");
                await events
                    .Pinged()
                    .SameRoomOnly()
                    .Where(r => r.ChatEventDetails.UserId == RobUserId)
                    .ForEachAsync(
                        async chatEvent =>
                        {
                            await RunWithLogging(async () =>
                            {
                                await ParseCommands(chatClient, chatEvent, dateService, cancellationToken);
                            });
                        }, cancellationToken);
            });
        }

        private async Task FollowInRoom(int roomId, int followingUserId, DateTime fromTime, CancellationToken cancellationToken)
        {
            await RunWithLogging(async () =>
            {
                var chatClient = _serviceProvider.GetRequiredService<ChatClient>();

                var questionIdRegex = new Regex(@"stackoverflow\.com\/q\/(\d+)");
                var userIdRegex = new Regex(@"\/users\/(\d+)");

                var events = chatClient.SubscribeToEvents(ChatSite.StackOverflow, roomId);
                await events.FirstAsync();
                _logger.LogInformation($"Successfully joined room {roomId}");
                chatClient.SendMessage(ChatSite.StackOverflow, Headquarters, $"I just joined {roomId}");
                await events
                    .OnlyMessages()
                    .SameRoomOnly()
                    .Where(r => r.ChatEventDetails.UserId == followingUserId)
                    .SlidingBuffer(TimeSpan.FromSeconds(30))
                    .ForEachAsync(async chatEvents =>
                    {
                        await RunWithLogging(async () =>
                        {
                            var questionIds =
                                chatEvents.SelectMany(ceg =>
                                        questionIdRegex
                                            .Matches(ceg.ChatEventDetails.Content)
                                            .Select(m => int.Parse(m.Groups[1].Value))
                                    )
                                    .Distinct();

                            foreach (var questionIdGroup in questionIds.Batch(100))
                            {
                                var questionIdList = questionIdGroup.ToList();
                                if (!questionIdList.Any())
                                    return;

                                chatClient.SendMessage(ChatSite.StackOverflow, Headquarters, $"Processing {string.Join(", ", questionIdList)} from room {roomId}");

                                var apiClient = _serviceProvider.GetRequiredService<ApiClient>();
                                var revisions = await apiClient.Revisions("stackoverflow.com", questionIdList);

                                var innerContext = _serviceProvider.GetRequiredService<RodgortContext>();
                                foreach (var revision in revisions.Items.Where(r => Dates.UnixTimeStampToDateTime(r.CreationDate) > fromTime)
                                )
                                {
                                    if (revision.LastTags != null)
                                    {
                                        // There was a retag
                                        var newTags = revision.LastTags.Except(revision.Tags);
                                        foreach (var newTag in newTags)
                                        {
                                            AddIfNew(new DbUserAction
                                            {
                                                UserActionTypeId = DbUserActionType.REMOVED_TAG,
                                                PostId = revision.PostId,
                                                Tag = newTag,
                                                Time = Dates.UnixTimeStampToDateTime(revision.CreationDate),
                                                SiteUserId = revision.User.UserId
                                            });
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(revision.LastBody) || !string.IsNullOrWhiteSpace(revision.LastTitle))
                                            continue;

                                        // There was a change that didn't edit the title, body or tags. Might be a closure
                                        if (revision.Comment == null || !revision.Comment.StartsWith("<b>Post Closed</b>"))
                                            continue;

                                        var userIds = userIdRegex
                                            .Matches(revision.Comment)
                                            .Select(m => int.Parse(m.Groups[1].Value))
                                            .ToList();

                                        foreach (var userId in userIds)
                                        {
                                            // A closure doesn't have a tags list.
                                            // So, check all previous revisions for this post and grab all the tags. A closure counts for all tags seen
                                            var allSeenTags = revisions.Items
                                                .Where(r => r.PostId == revision.PostId)
                                                .Where(r => r.CreationDate < revision.CreationDate)
                                                .Where(r => r.Tags != null).SelectMany(r => r.Tags).Distinct().ToList();
                                            foreach (var tag in allSeenTags)
                                            {
                                                AddIfNew(new DbUserAction
                                                {
                                                    UserActionTypeId = DbUserActionType.CLOSED,
                                                    Tag = tag,
                                                    PostId = revision.PostId,
                                                    Time = Dates.UnixTimeStampToDateTime(revision.CreationDate),
                                                    SiteUserId = userId
                                                });
                                            }
                                        }
                                    }
                                }

                                void AddIfNew(DbUserAction action)
                                {
                                    if (!innerContext.UserActions.Any(
                                        ua => ua.UserActionTypeId == action.UserActionTypeId
                                              && ua.Tag == action.Tag
                                              && ua.PostId == action.PostId
                                              && ua.Time == action.Time
                                              && ua.SiteUserId == action.SiteUserId
                                    ))
                                    {
                                        innerContext.UserActions.Add(action);
                                    }
                                }
                                
                                var currentUserIds = innerContext.UserActions.Local.Select(mqmt => mqmt.SiteUserId).Distinct().ToList();
                                var dbUsers = innerContext.SiteUsers.Where(t => currentUserIds.Contains(t.Id)).ToLookup(t => t.Id);
                                foreach (var currentUserId in currentUserIds)
                                {
                                    if (!dbUsers.Contains(currentUserId))
                                        innerContext.SiteUsers.Add(new DbSiteUser { Id = currentUserId });
                                }
                                
                                innerContext.SaveChanges();
                            }
                        });
                    }, cancellationToken);
            });
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
            if (!int.TryParse(args[0], out var burnakiUserId))
                return;
            if (!int.TryParse(args[1], out var roomId))
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
                    FollowStarted = dateService.UtcNow
                });
                innerContext.SaveChanges();

                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $"Okay, following {burnakiUserId} in {roomId}");

                FollowInRoom(roomId, burnakiUserId, DateTime.UtcNow, cancellationToken);
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
                innerContext.BurnakiFollows.Remove(existingFollow);
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
                var followMessage = $"I'm following: {string.Join(", ", allFollows.Select(f => $"{f.BurnakiId} in {f.RoomId}"))}";
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} {followMessage}");
            }
            else
            {
                await chatClient.SendMessage(ChatSite.StackOverflow, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} I'm not following anyone");
            }
        }

        private async Task RunWithLogging(Func<Task> task)
        {
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed chat: ", ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
