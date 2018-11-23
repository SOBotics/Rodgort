using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StackExchangeChat.Sites;
using StackExchangeChat.Utilities;

namespace StackExchangeChat
{
    public class SiteAuthenticator
    {
        private readonly HttpClientWithHandler _authenticatingHttpClient;
        private readonly HttpClientWithHandler _fkeyHttpClient;
        private readonly IChatCredentials _chatCredentials;

        private struct SiteRoomIdPair
        {
            public Site Site;
            public int RoomId;
        }

        private DateTime? _cookieExpires;
        private Task<Cookie> _authenticateTask;
        private readonly object _locker = new object();

        private readonly Dictionary<SiteRoomIdPair, Task<string>> m_CachedFKeys = new Dictionary<SiteRoomIdPair, Task<string>>();

        public SiteAuthenticator(
            HttpClientWithHandler authenticatingHttpClient,
            HttpClientWithHandler fkeyHttpClient,
            IChatCredentials chatCredentials)
        {
            if (ReferenceEquals(authenticatingHttpClient, fkeyHttpClient))
                throw new ArgumentException("Must provide two distinct instance of HttpClient for the Authenticating client and the fkey client.");
            
            _authenticatingHttpClient = authenticatingHttpClient;
            _fkeyHttpClient = fkeyHttpClient;
            _chatCredentials = chatCredentials;
        }

        public async Task AuthenticateClient(HttpClientWithHandler httpClient, Site site)
        {
            var acctCookie = await GetAccountCookie(site);

            var cookieContainer = new CookieContainer();
            httpClient.Handler.CookieContainer = cookieContainer;
            cookieContainer.Add(acctCookie);
        }

        public async Task<string> GetFKeyForRoom(Site site, int roomId)
        {
            Task<string> task;
            lock (_locker)
            {
                var pair = new SiteRoomIdPair {Site = site, RoomId = roomId};
                if (!m_CachedFKeys.ContainsKey(pair))
                    m_CachedFKeys[pair] = GetFKeyForRoomInternal();

                task = m_CachedFKeys[pair];
            }

            return await task;

            async Task<string> GetFKeyForRoomInternal()
            {
                await AuthenticateClient(_fkeyHttpClient, site);
                var result = await _fkeyHttpClient.GetAsync($"https://{site.ChatDomain}/rooms/{roomId}");
                var resultStr = await result.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(resultStr);

                var fkeyElement = doc.DocumentNode.SelectSingleNode("//input[@id = 'fkey']");
                var fkey = fkeyElement.Attributes["value"].Value;

                return fkey;
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
                if (!string.IsNullOrWhiteSpace(_chatCredentials.AcctCookie))
                {
                    var expiry = DateTime.ParseExact(_chatCredentials.AcctCookieExpiry, "yyyy-MM-dd HH:mm:ssZ", CultureInfo.InvariantCulture);
                    return new Cookie("acct", _chatCredentials.AcctCookie, "/", site.LoginDomain)
                    {
                        Expires = expiry
                    };
                }

                var cookieContainer = new CookieContainer();
                _authenticatingHttpClient.Handler.CookieContainer = cookieContainer;

                var result = await _authenticatingHttpClient.GetAsync($"https://{site.LoginDomain}/users/login");
                var content = await result.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var fkeyElement = doc.DocumentNode.SelectSingleNode("//form[@id = 'login-form']/input[@name = 'fkey']");
                var fkey = fkeyElement.Attributes["value"].Value;

                var loginPayload = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"fkey", fkey},
                    {"email", _chatCredentials.Email},
                    {"password", _chatCredentials.Password},
                });

                await _authenticatingHttpClient.PostAsync($"https://{site.LoginDomain}/users/login", loginPayload);
                var cookies = cookieContainer.GetCookies(new Uri($"https://{site.LoginDomain}")).Cast<Cookie>().ToList();
                var acctCookie = cookies.FirstOrDefault(c => c.Name == "acct");
                if (acctCookie == null || acctCookie.Expired)
                    throw new Exception();

                return acctCookie;
            }
        }
    }
}
