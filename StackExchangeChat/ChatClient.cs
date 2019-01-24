using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private readonly HttpClientWithHandler _httpClient;
        private readonly ILogger<ChatClient> _logger;

        public ChatClient(SiteAuthenticator siteAuthenticator, HttpClientWithHandler httpClient, ILogger<ChatClient> logger)
        {
            _siteAuthenticator = siteAuthenticator;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<int> SendMessageAndPin(ChatSite chatSite, int roomId, string message)
        {
            var messageId = await SendMessage(chatSite, roomId, message);
            await PinMessage(chatSite, roomId, messageId);
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

                    LogResponseError(response);

                    var responseString = await response.Content.ReadAsStringAsync();
                    var matchesBackoff = backoffRegex.Match(responseString);
                    if (matchesBackoff.Success)
                    {
                        var backoffSeconds = int.Parse(matchesBackoff.Groups[1].Value);
                        await Task.Delay(TimeSpan.FromSeconds(backoffSeconds + 5));

                        var messageId = await SendMessage();

                        await Task.Delay(TimeSpan.FromSeconds(15));

                        return messageId;
                    }

                    var responsePayload = JsonConvert.DeserializeObject<JObject>(responseString);
                    return int.Parse(responsePayload["id"].ToString());
                }

                return await SendMessage();
            }, _ => Task.Delay(TimeSpan.FromSeconds(5)));
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
            }, _ => Task.Delay(TimeSpan.FromSeconds(5)));
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

        public async Task PinMessage(ChatSite chatSite, int currentRoomId, int messageId)
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
            }, Task.Delay(TimeSpan.FromSeconds(5)));
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

        public IObservable<ChatEvent> SubscribeToEvents(ChatSite chatSite, int roomId)
        {
            return Observable.Create<ChatEvent>(async observer =>
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

                var wsAuthUrl = JsonConvert.DeserializeObject<JObject>(await wsAuthRequest.Content.ReadAsStringAsync())["url"].Value<string>();

                var eventsRequest = await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/chats/{roomId}/events",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"mode", "events"},
                            {"msgCount", "0"},
                            {"fkey", roomDetails.FKey}
                        }));

                var lastEventTime = JsonConvert.DeserializeObject<JObject>(await eventsRequest.Content.ReadAsStringAsync())["time"].Value<string>();

                var webSocket = new PlainWebSocket($"{wsAuthUrl}?l={lastEventTime}", new Dictionary<string, string> { { "Origin", $"https://{chatSite.ChatDomain}"} });
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

                return Disposable.Create(() =>
                {
                    // webSocket.Dispose();
                });
            });
        }

        private void LogResponseError(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
                _logger.LogError($"Request failed for {responseMessage.RequestMessage.RequestUri}: {responseMessage.StatusCode}. {responseMessage.ReasonPhrase}");
        }
    }
}
