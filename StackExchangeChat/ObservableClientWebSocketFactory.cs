using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StackExchangeChat
{
    public class ObservableClientWebSocketFactory
    {
        private readonly ILogger<ObservableClientWebSocketFactory> _logger;

        public ObservableClientWebSocketFactory(ILogger<ObservableClientWebSocketFactory> logger)
        {
            _logger = logger;
        }

        public async Task<ObservableClientWebSocket> Create(string endpoint, IDictionary<string, string> headers)
        {
            var websocket = new ObservableClientWebSocket(_logger, endpoint, headers);
            await websocket.Connect();
            return websocket;
        }
    }
}