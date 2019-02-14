using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reactive.Linq;
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
        private static readonly Dictionary<string, Func<WebSocket, CancellationTokenSource, Task>> _endPoints = new Dictionary<string, Func<WebSocket, CancellationTokenSource, Task>>
        {
            { "/ws/quotaRemaining", ProcessQuotaRemaining},
            { "/ws/pipelines", ProcessPipelinesStatus }
        };

        public static void ConfigureQuotaRemainingWebsocket(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (_endPoints.ContainsKey(context.Request.Path))
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var cancellationTokenSource = new CancellationTokenSource();

                        Observable.Interval(TimeSpan.FromMinutes(1)).Subscribe(async i =>
                        {
                            await SendData(webSocket, cancellationTokenSource, "ping");
                            await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[16]), CancellationToken.None);
                        });
                        
                        await _endPoints[context.Request.Path](webSocket, cancellationTokenSource);
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

        private static async Task ProcessQuotaRemaining(WebSocket websocket, CancellationTokenSource cancellationTokenSource)
        {
            // We're using ForEachAsync rather than Subscribe because I was unable to get reactivex to execute the observer code
            // On a context in which the websocket hadn't been closed. 
            await ApiClient.QuotaRemaining.ForEachAsync(async quotaRemaining =>
            {
                await SendData(websocket, cancellationTokenSource, new { quotaRemaining });
            }, cancellationTokenSource.Token);
        }

        public static async Task ProcessPipelinesStatus(WebSocket websocket,
            CancellationTokenSource cancellationTokenSource)
        {
            await GitlabWebhookController.PipelineStatus.ForEachAsync(async status =>
            {
                await SendData(websocket, cancellationTokenSource, new { status });
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
