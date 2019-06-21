using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StackExchangeChat
{
    public class ObservableClientWebSocketConnection
    {
        private readonly ClientWebSocket _webSocket;
        private readonly Func<Task<ObservableClientWebSocketConnection>> _websocketCreator;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger<ObservableClientWebSocket> _logger;

        private ObservableClientWebSocketConnection _recreatedConnection;

        private readonly Subject<string> _subject;
        private readonly IObservable<string> _observable;

        public ObservableClientWebSocketConnection(
            ClientWebSocket webSocket,
            Func<Task<ObservableClientWebSocketConnection>> websocketCreator,
            CancellationTokenSource cancellationTokenSource, 
            ILogger<ObservableClientWebSocket> logger)
        {
            _webSocket = webSocket;
            _websocketCreator = websocketCreator;
            _cancellationTokenSource = cancellationTokenSource;
            _logger = logger;

            _subject = new Subject<string>();
            _observable = _subject.AsObservable().Publish().RefCount();
            
            Task.Run(ProcessReads);
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
            if (_recreatedConnection != null)
            {
                await _recreatedConnection.Send(bytes, messageType);
                return;
            }

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
                        msgInfo = await _webSocket.ReceiveAsync(b, _cancellationTokenSource.Token);

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
                _logger.LogError("Failed reading message", ex);

                if (_webSocket.State == WebSocketState.CloseReceived || _webSocket.State == WebSocketState.Closed)
                {
                    _logger.LogWarning("Creating a new websocket, as previous was closed");
                    _recreatedConnection = await _websocketCreator();
                    _recreatedConnection.Messages().Subscribe(_subject);
                }
            }
        }
    }
}