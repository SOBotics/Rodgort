using System.Net.Http;

namespace StackExchangeChat.Utilities
{
    public class HttpClientWithHandler : HttpClient
    {
        public HttpClientHandler Handler { get; }

        public HttpClientWithHandler(HttpClientHandler handler) : base(handler)
        {
            Handler = handler;
        }
    }
}
