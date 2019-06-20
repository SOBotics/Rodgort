using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using Rodgort.Controllers;
using StackExchangeApi;

namespace Rodgort.Services
{
    public static class WebsocketService
    {
        private static readonly Dictionary<string, Action<BehaviorSubject<Func<WebSocket, CancellationTokenSource, Task>>>> _endPoints = new Dictionary<string, Action<BehaviorSubject<Func<WebSocket, CancellationTokenSource, Task>>>>
        {
            { "/ws/quotaRemaining", ProcessQuotaRemaining},
            { "/ws/pipelines", ProcessPipelinesStatus }
        };

        public static void ConfigureWebsockets(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (_endPoints.ContainsKey(context.Request.Path))
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        try
                        {
                            var taskSubscription = new BehaviorSubject<Func<WebSocket, CancellationTokenSource, Task>>((_, __) => Task.CompletedTask);
                            Observable.Interval(TimeSpan.FromMinutes(1)).Subscribe(i =>
                            {
                                taskSubscription.OnNext(async (ws, cts) =>
                                {
                                    try
                                    {
                                        if (!ws.CloseStatus.HasValue)
                                        {
                                            await SendData(ws, cts, "ping");
                                            if (!ws.CloseStatus.HasValue)
                                                await ws.ReceiveAsync(new ArraySegment<byte>(new byte[16]), cts.Token);
                                            else
                                                cts.Cancel();
                                        }
                                        else
                                            cts.Cancel();
                                    } catch (WebSocketException) { }
                                });
                            });

                            _endPoints[context.Request.Path](taskSubscription);

                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var cancellationTokenSource = new CancellationTokenSource();

                            await taskSubscription.ForEachAsync(async task => { await task(webSocket, cancellationTokenSource); }, cancellationTokenSource.Token);
                        } catch (TaskCanceledException) {  }
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }

        private static void ProcessQuotaRemaining(BehaviorSubject<Func<WebSocket, CancellationTokenSource, Task>> taskSubject)
        {
            ApiClient.QuotaRemaining.Subscribe(quotaRemaining =>
            {
                taskSubject.OnNext(async (websocket, cancellationTokenSource) => { await SendData(websocket, cancellationTokenSource, new {quotaRemaining}); });
            });
        }

        public static void ProcessPipelinesStatus(BehaviorSubject<Func<WebSocket, CancellationTokenSource, Task>> taskSubject)
        {
            GitlabWebhookController.PipelineStatus.Subscribe(status =>
            {
                taskSubject.OnNext(async (websocket, cancellationTokenSource) => { await SendData(websocket, cancellationTokenSource, new {status}); });
            });
        }

        private static async Task SendData(WebSocket websocket, CancellationTokenSource cancellationTokenSource, object payload)
        {
            var payloadStr = JsonConvert.SerializeObject(payload);
            await SendData(websocket, cancellationTokenSource, payloadStr);
        }

        private static async Task SendData(WebSocket websocket, CancellationTokenSource cancellationTokenSource, string payloadStr)
        {
            var payloadBytes = Encoding.UTF8.GetBytes(payloadStr);
            var bytes = new ArraySegment<byte>(payloadBytes);
            if (!websocket.CloseStatus.HasValue)
            {
                try
                {
                    await websocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception)
                {
                    cancellationTokenSource.Cancel();
                }
            }
            else
                cancellationTokenSource.Cancel();
        }
    }
}
