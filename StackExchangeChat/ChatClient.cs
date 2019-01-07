using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public ChatClient(SiteAuthenticator siteAuthenticator, HttpClientWithHandler httpClient)
        {
            _siteAuthenticator = siteAuthenticator;
            _httpClient = httpClient;
        }

        public async Task<int> SendMessage(ChatSite chatSite, int roomId, string message)
        {
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
                    var responsePayload = JsonConvert.DeserializeObject<JObject>(responseString);
                    return int.Parse(responsePayload["id"].ToString());
                }

                const int maxNumTries = 3;
                var currentTryCount = 0;

                try
                {
                    return await SendMessage();
                }
                catch (Exception)
                {
                    // We failed to deserialize the response, we probably got throttled.
                    await Task.Delay(TimeSpan.FromSeconds(15));

                    currentTryCount++;
                    if (currentTryCount >= maxNumTries)
                        throw;

                    return await SendMessage();
                }

            }, _ => Task.Delay(TimeSpan.FromSeconds(5)));
        }

        private readonly Regex _roomRegex = new Regex(@"\/rooms\/info\/(\d+)\/");
        public async Task<int> CreateRoom(ChatSite chatSite, int currentRoomId, string roomName, string roomDescription)
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
                            {"tags", string.Empty},
                            {"noDupeCheck", "true"}
                        }));

                var requestUri = response.RequestMessage.RequestUri;
                var roomId = int.Parse(_roomRegex.Match(requestUri.AbsolutePath).Groups[1].Value);
                return roomId;
            }, _ => Task.Delay(TimeSpan.FromSeconds(5)));
        }

        public async Task PinMessage(ChatSite chatSite, int currentRoomId, int messageId)
        {
            await ThrottlingUtils.Throttle(ChatThrottleGroups.WebRequestThrottle, async () =>
            {
                var fkey = (await _siteAuthenticator.GetRoomDetails(chatSite, currentRoomId)).FKey;
                await _siteAuthenticator.AuthenticateClient(_httpClient, chatSite);
                await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/messages/{messageId}/owner-star  ",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"fkey", fkey},
                        }));
            }, Task.Delay(TimeSpan.FromSeconds(5)));
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
    }
}
