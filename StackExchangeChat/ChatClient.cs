using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchangeChat.Console.AppSettings;
using StackExchangeChat.Sites;
using WebSocketSharp;

namespace StackExchangeChat
{
    public class ChatClient
    {
        private struct SiteRoomIdPair
        {
            public Site Site;
            public int RoomId;
        }

        private class WSAuthResult
        {
            public string Url { get; set; }
        }

        private class EventsResult
        {
            public string Time { get; set; }
        }
        
        private readonly IServiceProvider _serviceProvider;

        private DateTime? _cookieExpires;
        private Task<Cookie> _authenticateTask;
        private readonly object _locker = new object();

        private readonly Dictionary<SiteRoomIdPair, Task<string>> m_CachedFKeys = new Dictionary<SiteRoomIdPair, Task<string>>();

        public ChatClient(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SendMessage(Site site, int roomId, string message)
        {
            var fkey = await GetFKeyForRoom(site, roomId);
            using (var httpClient = await GetAuthenticatedHttpClient(site))
            {
                await httpClient.PostAsync($"https://{site.ChatDomain}/chats/{roomId}/messages/new",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            {"text", message},
                            {"fkey", fkey}
                        }));
            }
        }

        public IObservable<ChatEvent> SubscribeToEvents(Site site, int roomId)
        {
            return Observable.Create<ChatEvent>(async observer =>
            {
                var roomFKey = await GetFKeyForRoom(site, roomId);

                WSAuthResult wsAuthResult;
                EventsResult eventsResult;
                using (var httpClient = await GetAuthenticatedHttpClient(site))
                {
                    var wsAuthRequest = await httpClient.PostAsync($"https://{site.ChatDomain}/ws-auth",
                        new FormUrlEncodedContent(
                            new Dictionary<string, string>
                            {
                                {"fkey", roomFKey},
                                {"roomid", roomId.ToString()}
                            }));

                    wsAuthResult = JsonConvert.DeserializeObject<WSAuthResult>(await wsAuthRequest.Content.ReadAsStringAsync());

                    var eventsRequest = await httpClient.PostAsync($"https://{site.ChatDomain}/chats/{roomId}/events",
                        new FormUrlEncodedContent(
                            new Dictionary<string, string>
                            {
                                {"mode", "events"},
                                {"msgCount", "0"},
                                {"fkey", roomFKey}
                            }));

                    eventsResult = JsonConvert.DeserializeObject<EventsResult>(await eventsRequest.Content.ReadAsStringAsync());
                }

                var webSocket = new WebSocket($"{wsAuthResult.Url}?l={eventsResult.Time}") {Origin = $"https://{site.ChatDomain}"};
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
                
                return Disposable.Create(() => { });
            });
        }

        private async Task<HttpClient> GetAuthenticatedHttpClient(Site site)
        {
            var acctCookie = await GetAccountCookie(site);
            var httpClientWithHandler = _serviceProvider.GetService<HttpClientWithHandler>();
            var cookieContainer = new CookieContainer();
            httpClientWithHandler.HttpClientHandler.CookieContainer = cookieContainer;
            cookieContainer.Add(acctCookie);
            return httpClientWithHandler.HttpClient;
        }

        private async Task<string> GetFKeyForRoom(Site site, int roomId)
        {
            var pair = new SiteRoomIdPair {Site = site, RoomId = roomId};
            if (!m_CachedFKeys.ContainsKey(pair))
                m_CachedFKeys[pair] = GetFKeyForRoomInternal();

            return await m_CachedFKeys[pair];
            async Task<string> GetFKeyForRoomInternal()
            {
                using (var httpClient = await GetAuthenticatedHttpClient(site))
                {
                    var result = await httpClient.GetAsync($"https://{site.ChatDomain}/rooms/{roomId}");
                    var resultStr = await result.Content.ReadAsStringAsync();

                    var doc = new HtmlDocument();
                    doc.LoadHtml(resultStr);

                    var fkeyElement = doc.DocumentNode.SelectSingleNode("//input[@id = 'fkey']");
                    var fkey = fkeyElement.Attributes["value"].Value;

                    return fkey;
                }
            }
        }

        private async Task<Cookie> GetAccountCookie(Site site)
        {
            lock (_locker)
            {
                if (_cookieExpires.HasValue && _cookieExpires.Value < DateTime.UtcNow)
                    _authenticateTask = null;

                if (_authenticateTask == null)
                    _authenticateTask = GetAccountCookieInternal();
            }

            var cookie = await _authenticateTask;
            _cookieExpires = cookie.Expires;
            return cookie;

            async Task<Cookie> GetAccountCookieInternal()
            {
                var credentials = _serviceProvider.GetService<Credentials>();
                if (!string.IsNullOrWhiteSpace(credentials.AcctCookie))
                {
                    var expiry = DateTime.ParseExact(credentials.AcctCookieExpiry, "yyyy-MM-dd HH:mm:ssZ", CultureInfo.InvariantCulture);
                    return new Cookie("acct", credentials.AcctCookie, "/", site.LoginDomain)
                    {
                        Expires = expiry
                    };
                }

                using (var httpClientWithHandler = _serviceProvider.GetService<HttpClientWithHandler>())
                {
                    var cookieContainer = new CookieContainer();
                    httpClientWithHandler.HttpClientHandler.CookieContainer = cookieContainer;

                    var httpClient = httpClientWithHandler.HttpClient;

                    var result = await httpClient.GetAsync($"https://{site.LoginDomain}/users/login");
                    var content = await result.Content.ReadAsStringAsync();

                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);

                    var fkeyElement = doc.DocumentNode.SelectSingleNode("//form[@id = 'login-form']/input[@name = 'fkey']");
                    var fkey = fkeyElement.Attributes["value"].Value;

                    var loginPayload = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"fkey", fkey},
                        {"email", credentials.Email},
                        {"password", credentials.Password},
                    });

                    await httpClient.PostAsync($"https://{site.LoginDomain}/users/login", loginPayload);
                    var cookies = cookieContainer.GetCookies(new Uri($"https://{site.LoginDomain}")).Cast<Cookie>().ToList();
                    var acctCookie = cookies.FirstOrDefault(c => c.Name == "acct");
                    if (acctCookie == null || acctCookie.Expired)
                        throw new Exception();

                    return acctCookie;
                }
            }
        }
    }
}
