using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace StackExchangeChat
{
    public class ObservableClientWebSocket
    {
        private static TimeSpan _reconnectDuration = TimeSpan.FromMinutes(5);

        private readonly ILogger<ObservableClientWebSocketFactory> _logger;

        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;

        private readonly string _endpoint;
        private readonly IDictionary<string, string> _headers;
        private readonly IApplicationLifetime _appLifetime;

        private readonly Subject<string> _subject;
        private readonly IObservable<string> _observable;

        public ObservableClientWebSocket(ILogger<ObservableClientWebSocketFactory> logger,
            string endpoint, IDictionary<string, string> headers,
            IApplicationLifetime appLifetime)
        {
            _logger = logger;
            _endpoint = endpoint;
            _headers = headers;
            _appLifetime = appLifetime;

            _subject = new Subject<string>();
            _observable = _subject.AsObservable().Publish().RefCount();
        }

        public async Task Connect()
        {
            try
            {
                _webSocket = new ClientWebSocket();

                if (_headers != null)
                    foreach (var kv in _headers)
                        _webSocket.Options.SetRequestHeader(kv.Key, kv.Value);

                _cancellationTokenSource = new CancellationTokenSource();

                _logger.LogInformation($"Connecting to {_endpoint}: {JsonConvert.SerializeObject(_headers)}");

                await _webSocket.ConnectAsync(new Uri(_endpoint), _cancellationTokenSource.Token);

                _logger.LogInformation(
                    $"Successfully connected to {_endpoint}: {JsonConvert.SerializeObject(_headers)}");

                new Thread(async () => { await ProcessReads(); }).Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect");

                _appLifetime.StopApplication();
            }
        }

        public IObservable<string> Messages()
        {
            return _observable;
        }

        public async Task Send(string message)
        {
            if (_webSocket.State != WebSocketState.Open)
                throw new Exception("The WebSocket must be open before attempting to send a message.");

            _logger.LogTrace($"Sending message to websocket: {message}");

            var bytes = Encoding.UTF8.GetBytes(message);

            await Send(bytes, WebSocketMessageType.Text);
        }


        private async Task Send(byte[] bytes, WebSocketMessageType messageType)
        {
            if (_webSocket.State != WebSocketState.Open)
                throw new Exception("The WebSocket must be open before attempting to send a message.");

            var bytesSegment = new ArraySegment<byte>(bytes);

            await _webSocket.SendAsync(bytesSegment, messageType, true, _cancellationTokenSource.Token);
        }

        private async Task ProcessReads()
        {
            const int bufferSize = 8192;

            try
            {
                while (true)
                {
                    WebSocketReceiveResult msgInfo;
                    var buffers = new List<byte[]>();
                    do
                    {
                        var b = new ArraySegment<byte>(new byte[bufferSize]);
                        _logger.LogDebug($"Starting receive async on {_endpoint}: {JsonConvert.SerializeObject(_headers)}");
                        msgInfo = await _webSocket.ReceiveAsync(b, _cancellationTokenSource.Token);
                        _logger.LogDebug($"Finished receive async on {_endpoint}: {JsonConvert.SerializeObject(_headers)}");

                        var bArray = b.Array;
                        if (msgInfo.Count < bufferSize)
                            Array.Resize(ref bArray, msgInfo.Count);

                        buffers.Add(bArray);
                    } while (!msgInfo.EndOfMessage);

                    var buffer = buffers.SelectMany(b => b).ToArray();
                    if (msgInfo.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        _logger.LogTrace($"Received message from websocket: {text}");
                        _subject.OnNext(text);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed reading message");
                await Task.Delay(_reconnectDuration);
                await Connect();
            }
        }
    }
}
