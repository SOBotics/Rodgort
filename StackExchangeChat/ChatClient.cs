using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchangeChat.Utilities;
using StackExchangeChat.Utilities.Throttling;
using Utilities.Throttling;

namespace StackExchangeChat
{
    public class ChatClient
    {
        public const int MAX_MESSAGE_LENGTH = 500;

        private readonly SiteAuthenticator _siteAuthenticator;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClientWithHandler _httpClient;
        private readonly ILogger<ChatClient> _logger;

        private static readonly Dictionary<SiteAuthenticator.SiteRoomIdPair, IObservable<ChatEvent>> _roomSubscriptions = new Dictionary<SiteAuthenticator.SiteRoomIdPair, IObservable<ChatEvent>>();

        private static readonly TimeSpan _delayAfterMessage = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan _delayAfterThrottle = TimeSpan.FromSeconds(15);

        public ChatClient(SiteAuthenticator siteAuthenticator,
            IServiceProvider serviceProvider,
            HttpClientWithHandler httpClient, ILogger<ChatClient> logger)
        {
            _siteAuthenticator = siteAuthenticator;
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<int> SendMessageAndPin(ChatSite chatSite, int roomId, string message)
        {
            var messageId = await SendMessage(chatSite, roomId, message);
            await PinMessages(chatSite, roomId, messageId);
            return messageId;
        }

        public async Task<int> SendMessage(ChatSite chatSite, int roomId, string message)
        {
            var backoffRegex = new Regex(@"You can perform this action again in (\d+) seconds?");

            return await ThrottlingUtils.Throttle(ChatThrottleGroups.WebRequestThrottle, async () =>
            {
                async Task<int> SendMessage()
                {
                    var fkey = (await _siteAuthenticator.GetRoomDetails(chatSite, roomId)).FKey;
                    await _siteAuthenticator.AuthenticateClient(_httpClient, chatSite);
                    var response = await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/chats/{roomId}/messages/new",
                        new FormUrlEncodedContent(
                            new Dictionary<string, string>
                            {
                                {"text", message},
                                {"fkey", fkey}
                            }));
                    
                    var responseString = await response.Content.ReadAsStringAsync();
                    var matchesBackoff = backoffRegex.Match(responseString);
                    if (matchesBackoff.Success)
                    {
                        var backoffSeconds = int.Parse(matchesBackoff.Groups[1].Value);
                        await Task.Delay(TimeSpan.FromSeconds(backoffSeconds).Add(_delayAfterMessage));

                        var messageId = await SendMessage();

                        await Task.Delay(_delayAfterThrottle);

                        return messageId;
                    }

                    LogResponseError(response);

                    var responsePayload = JsonConvert.DeserializeObject<JObject>(responseString);
                    return int.Parse(responsePayload["id"].ToString());
                }

                return await SendMessage();
            }, _ => Task.Delay(_delayAfterMessage));
        }

        private readonly Regex _roomRegex = new Regex(@"\/rooms\/info\/(\d+)\/");
        public async Task<int> CreateRoom(ChatSite chatSite, int currentRoomId, string roomName, string roomDescription, IEnumerable<string> tags)
        {
            return await ThrottlingUtils.Throttle(ChatThrottleGroups.WebRequestThrottle, async () =>
            {
                var fkey = (await _siteAuthenticator.GetRoomDetails(chatSite, currentRoomId)).FKey;
                await _siteAuthenticator.AuthenticateClient(_httpClient, chatSite);
                var response = await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/rooms/save",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"fkey", fkey},
                            {"defaultAccess", "read-write"},
                            {"name", roomName},
                            {"description", roomDescription},
                            {"tags", string.Join(" ", tags)},
                            {"noDupeCheck", "true"}
                        }));
                LogResponseError(response);

                var requestUri = response.RequestMessage.RequestUri;
                var roomId = int.Parse(_roomRegex.Match(requestUri.AbsolutePath).Groups[1].Value);
                return roomId;
            }, _ => Task.Delay(_delayAfterMessage));
        }

        public async Task EditRoom(ChatSite chatSite, int roomId, string roomName, string roomDescription, IEnumerable<string> tags)
        {
            await ThrottlingUtils.Throttle(ChatThrottleGroups.WebRequestThrottle, async () =>
            {
                var fkey = (await _siteAuthenticator.GetRoomDetails(chatSite, roomId)).FKey;
                await _siteAuthenticator.AuthenticateClient(_httpClient, chatSite);

                var response = await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/rooms/save",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"fkey", fkey},
                            {"roomId", roomId.ToString()},
                            {"name", roomName},
                            {"description", roomDescription},
                            {"tags", string.Join(" ", tags)},
                        }));
                LogResponseError(response);
            });
        }

        public async Task PinMessages(ChatSite chatSite, int currentRoomId, params int[] messageIds)
        {
            var pinnedMessages = await PinnedMessages(chatSite, currentRoomId);
            var messagesToPin = messageIds.Except(pinnedMessages);
            foreach(var messageToPin in messagesToPin)
                await TogglePin(chatSite, currentRoomId, messageToPin);
        }

        public async Task UnpinMessages(ChatSite chatSite, int currentRoomId, params int[] messageIds)
        {
            var pinnedMessages = await PinnedMessages(chatSite, currentRoomId);
            var messagesToUnpin = messageIds.Intersect(pinnedMessages);
            foreach (var messageToUnpin in messagesToUnpin)
                await TogglePin(chatSite, currentRoomId, messageToUnpin);
        }

        public async Task<List<int>> PinnedMessages(ChatSite chatSite, int currentRoomId)
        {
            using (var httpClient = _serviceProvider.GetService<HttpClientWithHandler>())
            {
                var result = await httpClient.GetAsync($"https://{chatSite.ChatDomain}/chats/stars/{currentRoomId}");
                var resultStr = await result.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(resultStr);

                const string searchFor = "owner-star";
                const string idPrefix = "summary_";
                var pinnedMessageIds =
                    doc.DocumentNode.Descendants()
                        .Where(
                            x => x.Attributes.Contains("class")
                                 && x.Attributes["class"].Value.Split(' ').Any(v => v.Contains(searchFor))
                        )
                        .Select(n => n.ParentNode)
                        .Select(n => n.GetAttributeValue("id", string.Empty))
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Where(n => n.Length > idPrefix.Length)
                        .Select(n => n.Substring(idPrefix.Length))
                        .Select(n => {
                            if (int.TryParse(n, out var res))
                                return (int?)res;
                            return null;
                        })
                        .Where(n => n != null)
                        .Select(n => n.Value)
                        .ToList();

                return pinnedMessageIds;
            }
        }

        public async Task<bool> IsPinned(ChatSite chatSite, int currentRoomId, int messageId)
        {
            return (await PinnedMessages(chatSite, currentRoomId)).Contains(messageId);
        }

        public async Task TogglePin(ChatSite chatSite, int currentRoomId, int messageId)
        {
            await ThrottlingUtils.Throttle(ChatThrottleGroups.WebRequestThrottle, async () =>
            {
                var fkey = (await _siteAuthenticator.GetRoomDetails(chatSite, currentRoomId)).FKey;
                await _siteAuthenticator.AuthenticateClient(_httpClient, chatSite);
                var response = await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/messages/{messageId}/owner-star  ",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"fkey", fkey},
                        }));
                LogResponseError(response);
            }, Task.Delay(_delayAfterMessage));
        }

        public async Task AddRoomOwner(ChatSite chatSite, int roomId, int userId)
        {
            await ThrottlingUtils.Throttle(ChatThrottleGroups.WebRequestThrottle, async () =>
            {
                var fkey = (await _siteAuthenticator.GetRoomDetails(chatSite, roomId)).FKey;
                await _siteAuthenticator.AuthenticateClient(_httpClient, chatSite);
                var response = await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/rooms/setuseraccess/{roomId}",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"fkey", fkey},
                            {"userAccess", "owner"},
                            {"aclUserId", userId.ToString()},
                        }));
                LogResponseError(response);
            });
        }

        public async Task AddWriteAccess(ChatSite chatSite, int roomId, int userId)
        {
            await ThrottlingUtils.Throttle(ChatThrottleGroups.WebRequestThrottle, async () =>
            {
                var fkey = (await _siteAuthenticator.GetRoomDetails(chatSite, roomId)).FKey;
                await _siteAuthenticator.AuthenticateClient(_httpClient, chatSite);
                var response = await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/rooms/setuseraccess/{roomId}",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"fkey", fkey},
                            {"userAccess", "read-write"},
                            {"aclUserId", userId.ToString()},
                        }));
                LogResponseError(response);
            });
        }

        public IObservable<ChatEvent> SubscribeToEvents(ChatSite chatSite, params int[] roomIds)
        {
            var obs = Observable.Empty<ChatEvent>();
            foreach (var roomId in roomIds)
                obs = obs.Merge(SubscribeToEvents(chatSite, roomId));

            return obs;
        }

        public IObservable<ChatEvent> SubscribeToEvents(ChatSite chatSite, int roomId)
        {
            var pair = new SiteAuthenticator.SiteRoomIdPair {ChatSite = chatSite, RoomId = roomId};
            lock (_roomSubscriptions)
            {
                if (!_roomSubscriptions.ContainsKey(pair))
                {
                    _roomSubscriptions[pair] = Observable.Create<ChatEvent>(async observer =>
                    {
                        var roomDetails = await _siteAuthenticator.GetRoomDetails(chatSite, roomId);

                        await _siteAuthenticator.AuthenticateClient(_httpClient, chatSite);
                        var wsAuthRequest = await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/ws-auth",
                            new FormUrlEncodedContent(
                                new Dictionary<string, string>
                                {
                                    {"fkey", roomDetails.FKey},
                                    {"roomid", roomId.ToString()}
                                }));

                        var wsAuthUrl =
                            JsonConvert.DeserializeObject<JObject>(await wsAuthRequest.Content.ReadAsStringAsync())
                                ["url"].Value<string>();

                        var eventsRequest = await _httpClient.PostAsync(
                            $"https://{chatSite.ChatDomain}/chats/{roomId}/events",
                            new FormUrlEncodedContent(
                                new Dictionary<string, string>
                                {
                                    {"mode", "events"},
                                    {"msgCount", "0"},
                                    {"fkey", roomDetails.FKey}
                                }));

                        var lastEventTime = JsonConvert.DeserializeObject<JObject>(await eventsRequest.Content.ReadAsStringAsync())["time"].Value<string>();

                        var webSocket = new PlainWebSocket($"{wsAuthUrl}?l={lastEventTime}", new Dictionary<string, string> {{"Origin", $"https://{chatSite.ChatDomain}"}}, _serviceProvider.GetRequiredService<ILogger<PlainWebSocket>>());
                        webSocket.OnTextMessage += (message) =>
                        {
                            var dataObject = JsonConvert.DeserializeObject<JObject>(message);
                            var eventsObject = dataObject.First.First["e"];
                            if (eventsObject == null)
                                return;

                            var events = eventsObject.ToObject<List<ChatEventDetails>>();
                            foreach (var @event in events)
                            {
                                var chatEvent = new ChatEvent
                                {
                                    RoomDetails = roomDetails,
                                    ChatEventDetails = @event,
                                    ChatClient = this
                                };
                                observer.OnNext(chatEvent);
                            }
                        };

                        await webSocket.ConnectAsync();

                        observer.OnNext(new ChatEvent
                        {
                            ChatEventDetails = new ChatEventDetails
                            {
                                ChatEventType = ChatEventType.ChatJoined,
                                RoomId = roomId
                            },
                            RoomDetails = roomDetails,
                            ChatClient = this
                        });

                        return webSocket;
                    }).Publish().RefCount();
                }

                return _roomSubscriptions[pair];
            }
        }

        private void LogResponseError(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
                _logger.LogError($"Request failed for {responseMessage.RequestMessage.RequestUri}: {responseMessage.StatusCode}. {responseMessage.ReasonPhrase}");
        }
    }
}
