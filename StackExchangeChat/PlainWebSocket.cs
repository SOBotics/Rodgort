﻿using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchangeChat
{
    // https://github.com/SOBotics/SharpExchange/blob/master/SharpExchange/Net/WebSocket/DefaultWebSocket.cs
    public class PlainWebSocket : IDisposable
    {
        private const int BufferSize = 4 * 1024;
        private ClientWebSocket _socket;
        private readonly CancellationTokenSource _socketTokenSource;
        private bool _dispose;

        public event Action OnOpen;
        public event Action<string> OnTextMessage;
        public event Action<byte[]> OnBinaryMessage;
        public event Action OnClose;
        public event Action<Exception> OnError;
        public event Action OnReconnectFailed;

        public Uri Endpoint { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public bool AutoReconnect { get; set; } = true;

        public PlainWebSocket(string endpoint, IReadOnlyDictionary<string, string> headers = null)
        {
            ThrowIfNullOrEmpty(endpoint, nameof(endpoint));

            _socketTokenSource = new CancellationTokenSource();

            Endpoint = new Uri(endpoint);
            Headers = headers;
        }

        public void Dispose()
        {
            if (_dispose)
                return;

            _dispose = true;

            if (_socket?.State == WebSocketState.Open)
                _socketTokenSource.Cancel();
            
            _socket?.Dispose();
        }

        public async Task ConnectAsync()
        {
            if (_dispose) return;

            if (_socket?.State == WebSocketState.Open || _socket?.State == WebSocketState.Connecting)
                throw new Exception("WebSocket is already open/connecting.");

            _socket = new ClientWebSocket();

            if (Headers != null)
                foreach (var kv in Headers)
                    _socket.Options.SetRequestHeader(kv.Key, kv.Value);
            
            await _socket.ConnectAsync(Endpoint, _socketTokenSource.Token);

            new Thread(Listen).Start();
            InvokeAsync(OnOpen);
        }
        
        private void Listen()
        {
            while (!_dispose)
            {
                var buffers = new List<byte[]>();
                WebSocketReceiveResult msgInfo = null;

                try
                {
                    while (!msgInfo?.EndOfMessage ?? true)
                    {
                        var b = new ArraySegment<byte>(new byte[BufferSize]);

                        msgInfo = _socket.ReceiveAsync(b, _socketTokenSource.Token).Result;

                        var bArray = b.Array;

                        Array.Resize(ref bArray, msgInfo.Count);

                        buffers.Add(bArray);
                    }
                }
                catch (AggregateException ex)
                when (ex.InnerException?.GetType() == typeof(TaskCanceledException))
                {
                    InvokeAsync(OnClose);
                    
                    return;
                }
                catch (Exception e1)
                {
                    OnError?.Invoke(e1);

                    if (!AutoReconnect) return;

                    try
                    {
                        _socketTokenSource.Token.WaitHandle.WaitOne(1000);

                        ConnectAsync().Wait();
                    }
                    catch (Exception e2)
                    {
                        InvokeAsync(OnReconnectFailed);
                        InvokeAsync(OnError, e2);
                        InvokeAsync(OnClose);
                    }

                    return;
                }

                var buffer = new List<byte>();

                foreach (var b in buffers)
                {
                    buffer.AddRange(b);
                }

                Task.Run(() => HandleNewMessage(msgInfo, buffer.ToArray()));
            }

            InvokeAsync(OnClose);
        }

        private void HandleNewMessage(WebSocketReceiveResult msgInfo, byte[] buffer)
        {
            if (msgInfo == null) return;

            try
            {
                if (msgInfo.MessageType == WebSocketMessageType.Text)
                {
                    var text = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                    OnTextMessage?.Invoke(text);
                }
                else if (msgInfo.MessageType == WebSocketMessageType.Binary)
                {
                    OnBinaryMessage?.Invoke(buffer);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        public static void InvokeAsync(Action del)
        {
            if (del == null)
                return;
            new Thread(del.Invoke).Start();
        }

        public static void InvokeAsync<T>(Action<T> del, T arg)
        {
            if (del == null)
                return;
            new Thread(() => del.Invoke(arg)).Start();
        }

        public static void ThrowIfNullOrEmpty(string str, string argName)
        {
            if (string.IsNullOrEmpty(argName))
            {
                throw new ArgumentException($"'{argName}' cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException($"'{argName}' cannot be null or empty.");
            }
        }
    }
}