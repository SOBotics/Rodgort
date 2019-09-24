using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace StackExchangeChat
{
    public class ObservableClientWebSocketFactory
    {
        private readonly ILogger<ObservableClientWebSocketFactory> _logger;
        private readonly IApplicationLifetime _appLifetime;

        public ObservableClientWebSocketFactory(ILogger<ObservableClientWebSocketFactory> logger, IApplicationLifetime appLifetime)
        {
            _logger = logger;
            _appLifetime = appLifetime;
        }

        public async Task<ObservableClientWebSocket> Create(string endpoint, IDictionary<string, string> headers)
        {
            var websocket = new ObservableClientWebSocket(_logger, endpoint, headers, _appLifetime);
            await websocket.Connect();
            return websocket;
        }
    }
}