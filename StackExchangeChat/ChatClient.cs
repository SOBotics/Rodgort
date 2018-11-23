using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchangeChat.Authenticators;
using StackExchangeChat.Sites;
using StackExchangeChat.Utilities;
using WebSocketSharp;

namespace StackExchangeChat
{
    public class ChatClient
    {
        private readonly SiteAuthenticator _siteAuthenticator;
        private readonly HttpClientWithHandler _httpClient;

        public ChatClient(SiteAuthenticator siteAuthenticator, HttpClientWithHandler httpClient)
        {
            _siteAuthenticator = siteAuthenticator;
            _httpClient = httpClient;
        }

        public async Task SendMessage(Site site, int roomId, string message)
        {
            var fkey = await _siteAuthenticator.GetFKeyForRoom(site, roomId);
            await _siteAuthenticator.AuthenticateClient(_httpClient, site);
            await _httpClient.PostAsync($"https://{site.ChatDomain}/chats/{roomId}/messages/new",
                new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        {"text", message},
                        {"fkey", fkey}
                    }));
        }

        public IObservable<ChatEvent> SubscribeToEvents(Site site, int roomId)
        {
            return Observable.Create<ChatEvent>(async observer =>
            {
                var roomFKey = await _siteAuthenticator.GetFKeyForRoom(site, roomId);

                await _siteAuthenticator.AuthenticateClient(_httpClient, site);
                var wsAuthRequest = await _httpClient.PostAsync($"https://{site.ChatDomain}/ws-auth",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"fkey", roomFKey},
                            {"roomid", roomId.ToString()}
                        }));

                var wsAuthUrl = JsonConvert.DeserializeObject<JObject>(await wsAuthRequest.Content.ReadAsStringAsync())["url"].Value<string>();

                var eventsRequest = await _httpClient.PostAsync($"https://{site.ChatDomain}/chats/{roomId}/events",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"mode", "events"},
                            {"msgCount", "0"},
                            {"fkey", roomFKey}
                        }));

                var lastEventTime = JsonConvert.DeserializeObject<JObject>(await eventsRequest.Content.ReadAsStringAsync())["time"].Value<string>();

                var webSocket = new WebSocket($"{wsAuthUrl}?l={lastEventTime}") {Origin = $"https://{site.ChatDomain}"};
                webSocket.OnMessage += (sender, args) =>
                {
                    var dataObject = JsonConvert.DeserializeObject<JObject>(args.Data);
                    var eventsObject = dataObject.First.First["e"];
                    if (eventsObject == null)
                        return;

                    var events = eventsObject.ToObject<List<ChatEvent>>();
                    foreach (var @event in events)
                        observer.OnNext(@event);
                };

                webSocket.Connect();
                
                return Disposable.Create(() =>
                {
                    webSocket.Close();
                });
            });
        }
    }
}
