using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        private readonly IServiceProvider _serviceProvider;
        public BurnakiFollowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var chatClient = scope.ServiceProvider.GetRequiredService<ChatClient>();
                var dateService = scope.ServiceProvider.GetRequiredService<DateService>();

                using (var context = scope.ServiceProvider.GetRequiredService<RodgortContext>())
                {
                    var burnakiFollows = context.BurnakiFollows.Where(bf => !bf.FollowEnded.HasValue).ToList();
                    foreach (var burnakiFollow in burnakiFollows)
                        FollowInRoom(burnakiFollow.RoomId, burnakiFollow.BurnakiId, burnakiFollow.FollowStarted, cancellationToken);
                }

                await chatClient.SubscribeToEvents(ChatSite.StackOverflow, 167908)
                    .Pinged()
                    .SameRoomOnly()
                    .Where(r => r.ChatEventDetails.UserId == 563532)
                    .ForEachAsync(async m =>
                    {
                        var splitContent = m.ChatEventDetails.Content.Split(" ");
                        if (splitContent.Length != 4)
                            return;
                        if (!string.Equals(splitContent[1], "follow"))
                            return;

                        if (!int.TryParse(splitContent[2], out var roomId))
                            return;
                        if (!int.TryParse(splitContent[2], out var burnakiUserId))
                            return;

                        using (var context = scope.ServiceProvider.GetRequiredService<RodgortContext>())
                        {
                            if (context.BurnakiFollows.Any(bf => bf.BurnakiId == burnakiUserId && bf.RoomId == roomId && !bf.FollowEnded.HasValue))
                            {
                                await chatClient.SendMessage(ChatSite.StackOverflow, 167908, $":{m.ChatEventDetails.UserId} That follow is already registered!");
                            }
                            else
                            {
                                context.BurnakiFollows.Add(new DbBurnakiFollow
                                {
                                    BurnakiId = burnakiUserId,
                                    RoomId = roomId,
                                    FollowStarted = dateService.UtcNow
                                });
                                context.SaveChanges();

                                await chatClient.SendMessage(ChatSite.StackOverflow, 167908, $"Okay, following {burnakiUserId} in {roomId}");

                                FollowInRoom(roomId, burnakiUserId, DateTime.UtcNow, cancellationToken);
                            }
                        }
                    }, cancellationToken);
            }
        }

        private async Task FollowInRoom(int roomId, int userId, DateTime fromTime, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var chatClient = scope.ServiceProvider.GetRequiredService<ChatClient>();
                var dateService = scope.ServiceProvider.GetRequiredService<DateService>();

                var questionIdRegex = new Regex(@"stackoverflow\.com\/q\/(\d+)");

                await chatClient.SubscribeToEvents(ChatSite.StackOverflow, roomId)
                    .OnlyMessages()
                    .SameRoomOnly()
                    .Where(r => r.ChatEventDetails.UserId == userId || r.ChatEventDetails.UserId == 563532)
                    // .SlidingBuffer(TimeSpan.FromSeconds(30))
                    .ForEachAsync(async chatEvent =>
                    {
                        var message = chatEvent.ChatEventDetails.Content;
                        var isTagRemoval = message.StartsWith("&#91; <a href=\"//stackapps.com/q/7027\">Burnaki</a> &#93; Tag removed:");
                        var isDeletion = message.StartsWith("\\[ [Burnaki](//stackapps.com/q/7027) \\] Deleted: ");

                        var questionIds = questionIdRegex
                            .Matches(message)
                            .Select(m => int.Parse(m.Groups[1].Value));

                        if (isTagRemoval)
                        {
                            var apiClient = scope.ServiceProvider.GetRequiredService<ApiClient>();
                            var revisions = await apiClient.Revisions("stackoverflow.com", questionIds);

                            using (var context = scope.ServiceProvider.GetRequiredService<RodgortContext>())
                            {
                                foreach (var revision in revisions.Items.Where(r => r.LastTags != null && r.Tags != null)
                                    .Where(r => Dates.UnixTimeStampToDateTime(r.CreationDate) > fromTime))
                                {
                                    var newTags = revision.LastTags.Except(revision.Tags);
                                    foreach (var newTag in newTags)
                                    {
                                        context.UserRetags.Add(new DbUserRetag
                                        {
                                            Tag = newTag,
                                            Removed = true,
                                            Time = dateService.UtcNow,
                                            UserId = revision.User.UserId
                                        });
                                    }
                                }

                                context.SaveChanges();
                            }
                        }


                    }, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
