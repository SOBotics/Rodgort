using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchangeChat.Utilities;
using WebSocketSharp;

namespace StackExchangeChat
{
    public class ChatClient
    {
        private readonly SiteAuthenticator _siteAuthenticator;
        private readonly HttpClientWithHandler _httpClient;

        public static object TaskLocker = new object();
        public static Task ExecutingTask = Task.CompletedTask;

        public ChatClient(SiteAuthenticator siteAuthenticator, HttpClientWithHandler httpClient)
        {
            _siteAuthenticator = siteAuthenticator;
            _httpClient = httpClient;
        }

        public async Task SendMessage(ChatSite chatSite, int roomId, string message)
        {
            Task nextMessageTask;
            lock (TaskLocker)
            {
                if (ExecutingTask.IsFaulted)
                    ExecutingTask = Task.CompletedTask;

                nextMessageTask = SendMessageInternal(ExecutingTask);
                ExecutingTask = PostProcess(nextMessageTask);
            }

            await nextMessageTask;

            async Task PostProcess(Task executingTask)
            {
                await executingTask;
                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            async Task SendMessageInternal(Task previousTask)
            {
                await previousTask;
                var fkey = (await _siteAuthenticator.GetRoomDetails(chatSite, roomId)).FKey;
                await _siteAuthenticator.AuthenticateClient(_httpClient, chatSite);
                await _httpClient.PostAsync($"https://{chatSite.ChatDomain}/chats/{roomId}/messages/new",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"text", message},
                            {"fkey", fkey}
                        }));
            }
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

                var webSocket = new WebSocket($"{wsAuthUrl}?l={lastEventTime}") {Origin = $"https://{chatSite.ChatDomain}"};
                webSocket.OnMessage += (sender, args) =>
                {
                    var dataObject = JsonConvert.DeserializeObject<JObject>(args.Data);
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

                webSocket.Connect();
                
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
                    webSocket.Close();
                });
            });
        }
    }
}
