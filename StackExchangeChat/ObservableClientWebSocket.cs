using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace StackExchangeChat
{
    public class ObservableClientWebSocket
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ObservableClientWebSocket> _logger;

        public ObservableClientWebSocket(IServiceProvider serviceProvider, ILogger<ObservableClientWebSocket> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<ObservableClientWebSocketConnection> Connect(string endpoint, IDictionary<string, string> headers)
        {
            async Task<ObservableClientWebSocketConnection> WebsocketCreation()
            {
                var socket = new ClientWebSocket();

                if (headers != null)
                    foreach (var kv in headers)
                        socket.Options.SetRequestHeader(kv.Key, kv.Value);

                var cancellationTokenSource = new CancellationTokenSource();

                _logger.LogInformation($"Connecting to {endpoint}: {JsonConvert.SerializeObject(headers)}");

                await socket.ConnectAsync(new Uri(endpoint), cancellationTokenSource.Token);

                _logger.LogInformation($"Successfully connected to {endpoint}: {JsonConvert.SerializeObject(headers)}");

                return new ObservableClientWebSocketConnection(socket, WebsocketCreation, cancellationTokenSource, _serviceProvider.GetService<ILogger<ObservableClientWebSocket>>());
            }

            return await WebsocketCreation();
        }

    }
}