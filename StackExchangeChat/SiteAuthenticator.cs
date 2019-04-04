using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using StackExchangeChat.Utilities;

namespace StackExchangeChat
{
    public class SiteAuthenticator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCredentials _chatCredentials;

        public struct SiteRoomIdPair { public ChatSite ChatSite; public int RoomId; }
        private readonly Dictionary<ChatSite, DateTime> _cookieExpires = new Dictionary<ChatSite, DateTime>();
        private readonly Dictionary<ChatSite, Task<Cookie>> _authenticateTasks = new Dictionary<ChatSite, Task<Cookie>>();
        private readonly object _locker = new object();

        private static readonly Dictionary<SiteRoomIdPair, Task<RoomDetails>> _cachedRoomDetails = new Dictionary<SiteRoomIdPair, Task<RoomDetails>>();

        public SiteAuthenticator(IServiceProvider serviceProvider, IChatCredentials chatCredentials)
        {
            _serviceProvider = serviceProvider;
            _chatCredentials = chatCredentials;
        }

        public async Task AuthenticateClient(HttpClientWithHandler httpClient, ChatSite chatSite)
        {
            var acctCookie = await GetAccountCookie(chatSite);
            httpClient.Handler.CookieContainer.Add(acctCookie);
        }

        public async Task<RoomDetails> GetRoomDetails(ChatSite chatSite, int roomId)
        {
            Task<RoomDetails> task;
            lock (_locker)
            {
                var pair = new SiteRoomIdPair {ChatSite = chatSite, RoomId = roomId};
                if (!_cachedRoomDetails.ContainsKey(pair))
                    _cachedRoomDetails[pair] = GetFKeyForRoomInternal();

                task = _cachedRoomDetails[pair];
            }

            return await task;

            async Task<RoomDetails> GetFKeyForRoomInternal()
            {
                using (var httpClient = _serviceProvider.GetService<HttpClientWithHandler>())
                {
                    await AuthenticateClient(httpClient, chatSite);
                    var result = await httpClient.GetAsync($"https://{chatSite.ChatDomain}/rooms/{roomId}");
                    var resultStr = await result.Content.ReadAsStringAsync();

                    var doc = new HtmlDocument();
                    doc.LoadHtml(resultStr);

                    var fkeyElement = doc.DocumentNode.SelectSingleNode("//input[@id = 'fkey']");
                    var fkey = fkeyElement.Attributes["value"].Value;

                    var userPanel = doc.DocumentNode.SelectSingleNode("//div[@id = 'active-user']");
                    var classItems = userPanel.Attributes["class"].Value;

                    var userIdRegex = new Regex("user\\-(\\d+)");
                    var userId = int.Parse(userIdRegex.Match(classItems).Groups[1].Value);
                    var userName = userPanel.SelectSingleNode(".//img").Attributes["title"].Value;
                    
                    var roomDetails = new RoomDetails
                    {
                        ChatSite = chatSite,
                        RoomId = roomId,
                        
                        MyUserId = userId,
                        MyUserName = userName,

                        FKey = fkey
                    };

                    return roomDetails;
                }
            }
        }

        private async Task<Cookie> GetAccountCookie(ChatSite chatSite)
        {
            Task<Cookie> task;
            lock (_locker)
            {
                if (_cookieExpires.ContainsKey(chatSite) && _cookieExpires[chatSite] < DateTime.UtcNow)
                    _authenticateTasks.Remove(chatSite);

                if (!_authenticateTasks.ContainsKey(chatSite))
                    _authenticateTasks[chatSite] = GetAccountCookieInternal();

                task = _authenticateTasks[chatSite];
            }

            var cookie = await task;
            lock(_locker)
                _cookieExpires[chatSite] = cookie.Expires;

            return cookie;

            async Task<Cookie> GetAccountCookieInternal()
            {
                if (!string.IsNullOrWhiteSpace(_chatCredentials.AcctCookie))
                    return new Cookie("acct", _chatCredentials.AcctCookie, "/", chatSite.LoginDomain);

                var cookieContainer = new CookieContainer();
                using (var httpClient = _serviceProvider.GetService<HttpClientWithHandler>())
                {
                    httpClient.Handler.CookieContainer = cookieContainer;

                    var result = await httpClient.GetAsync($"https://{chatSite.LoginDomain}/users/login");
                    var content = await result.Content.ReadAsStringAsync();

                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);

                    var fkeyElement =
                        doc.DocumentNode.SelectSingleNode("//form[@id = 'login-form']/input[@name = 'fkey']");
                    var fkey = fkeyElement.Attributes["value"].Value;

                    var loginPayload = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"fkey", fkey},
                        {"email", _chatCredentials.Email},
                        {"password", _chatCredentials.Password}
                    });

                    await httpClient.PostAsync($"https://{chatSite.LoginDomain}/users/login", loginPayload);
                    var cookies = cookieContainer.GetCookies(new Uri($"https://{chatSite.LoginDomain}")).Cast<Cookie>().ToList();
                    var acctCookie = cookies.FirstOrDefault(c => c.Name == "acct");
                    if (acctCookie == null || acctCookie.Expired)
                        throw new Exception();

                    return acctCookie;
                }
            }
        }
    }
}
