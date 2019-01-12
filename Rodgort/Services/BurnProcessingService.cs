﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Utilities;
using StackExchangeApi;
using StackExchangeChat;

namespace Rodgort.Services
{
    public class BurnProcessingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCredentials _chatCredentials;
        private readonly ChatClient _chatClient;
        private readonly DateService _dateService;
        private readonly ILogger<BurnakiFollowService> _logger;

        public BurnProcessingService(
            IServiceProvider serviceProvider, 
            IChatCredentials chatCredentials, 
            ChatClient chatClient,
            DateService dateService,
            ILogger<BurnakiFollowService> logger)
        {
            _serviceProvider = serviceProvider;
            _chatCredentials = chatCredentials;
            _chatClient = chatClient;
            _dateService = dateService;
            _logger = logger;
        }

        private static readonly Regex USER_ID_REGEX = new Regex(@"\/users\/(\d+)");

        public async Task ProcessQuestionIds(IEnumerable<int> questionIds, string followingTag, 
            DateTime? fromTime, int? roomId, bool announce = true)
        {
            var distinctQuestionIds = questionIds.Distinct();
            foreach (var questionIdGroup in distinctQuestionIds.Batch(100))
            {
                var questionIdList = questionIdGroup.ToList();
                if (!questionIdList.Any())
                    return;

                var messages = new List<string>();
                var tempIds = new List<int>();
                foreach (var questionId in questionIdList)
                {
                    var message = GetProcessingMessage(tempIds.Concat(new[] { questionId }));
                    if (message.Length > ChatClient.MAX_MESSAGE_LENGTH)
                    {
                        messages.Add(GetProcessingMessage(tempIds));
                        tempIds.Clear();
                    }

                    tempIds.Add(questionId);
                }

                if (tempIds.Any())
                    messages.Add(GetProcessingMessage(tempIds));

                string GetProcessingMessage(IEnumerable<int> qIds)
                {
                    return roomId.HasValue
                        ? $"Processing {string.Join(", ", qIds)} from room {roomId}"
                        : $"Processing {string.Join(", ", qIds)} from manual start";
                }

                if (announce && !string.IsNullOrEmpty(_chatCredentials.AcctCookie) || (!string.IsNullOrEmpty(_chatCredentials.Email) && !string.IsNullOrEmpty(_chatCredentials.Password)))
                {
                    foreach (var message in messages)
                        await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.HEADQUARTERS, message);
                }

                var apiClient = _serviceProvider.GetRequiredService<ApiClient>();
                var revisions = await apiClient.Revisions("stackoverflow.com", questionIdList);

                var innerContext = _serviceProvider.GetRequiredService<RodgortContext>();
                foreach (var revision in revisions.Items)
                {
                    if (fromTime.HasValue && Dates.UnixTimeStampToDateTime(revision.CreationDate) <= fromTime)
                    {
                        _logger.LogInformation($"Skipping revision {revision.RevisionNumber} on {revision.PostId}, as it's too old");
                        continue;
                    }

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

                        var userIds = USER_ID_REGEX
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

                var returnedItemLookup = revisions.Items.Select(i => i.PostId).Distinct().ToLookup(a => a);
                foreach (var questionId in questionIdList)
                {
                    if (!returnedItemLookup.Contains(questionId))
                    {
                        if (!innerContext.UserActions.Any(ua =>
                            ua.PostId == questionId
                            && ua.UserActionTypeId == DbUserActionType.UNKNOWN_DELETION
                            && ua.Tag == followingTag))
                        {
                            innerContext.UserActions.Add(
                                new DbUserAction
                                {
                                    UserActionTypeId = DbUserActionType.UNKNOWN_DELETION,
                                    Tag = followingTag,
                                    PostId = questionId,
                                    Time = _dateService.UtcNow,
                                    SiteUserId = -1
                                });
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

                var hasNewUser = false;
                foreach (var currentUserId in currentUserIds)
                {
                    if (!dbUsers.Contains(currentUserId))
                    {
                        innerContext.SiteUsers.Add(new DbSiteUser { Id = currentUserId });
                        hasNewUser = true;
                    }
                }

                _logger.LogInformation("Saving user actions...");
                innerContext.SaveChanges();
                _logger.LogInformation("User actions saved");

                if (hasNewUser)
                    RecurringJob.Trigger(UserDisplayNameService.SYNC_USERS_NO_NAME);
            }
        }
    }
}
