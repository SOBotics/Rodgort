using System;
using System.Net.Http;

namespace StackExchangeChat
{
    public class HttpClientWithHandler : IDisposable
    {
        public HttpClient HttpClient { get; set; }
        public HttpClientHandler HttpClientHandler { get; set; }

        public void Dispose()
        {
            HttpClient?.Dispose();
            HttpClientHandler?.Dispose();
        }
    }
}
