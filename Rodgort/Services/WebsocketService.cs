using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rodgort.Controllers;
using StackExchangeApi;

namespace Rodgort.Services
{
    public static class WebsocketService
    {
        private static readonly Dictionary<string, IObservable<Func<WebSocket, CancellationTokenSource, Task>>> _readWriteEndpoint = new Dictionary<string, IObservable<Func<WebSocket, CancellationTokenSource, Task>>>
            {
            };

        private static readonly Dictionary<string, IObservable<object>> _readonlyEndpoint = new Dictionary<string, IObservable<object>>
        {
            { "/ws/quotaRemaining", ApiClient.QuotaRemaining.Select(quotaRemaining => new { quotaRemaining }) },
            { "/ws/pipelines", GitlabWebhookController.PipelineStatus.Select(status => new { status }) }
        };

        public static void ConfigureWebsockets(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (_readonlyEndpoint.ContainsKey(context.Request.Path) || _readWriteEndpoint.ContainsKey(context.Request.Path))
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var scope = context.RequestServices.CreateScope();
                        var logger = scope.ServiceProvider.GetService<ILogger<object>>();
                        try
                        {
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var cancellationTokenSource = new CancellationTokenSource();

                            IObservable<Func<Task>> data;
                            if (_readonlyEndpoint.ContainsKey(context.Request.Path))
                                data = ProcessReadOnly(_readonlyEndpoint[context.Request.Path], webSocket,
                                    cancellationTokenSource);
                            else if (_readWriteEndpoint.ContainsKey(context.Request.Path))
                                data = ProcessReadWrite(_readWriteEndpoint[context.Request.Path], webSocket,
                                    cancellationTokenSource);
                            else
                                return;

                            var pinger = ProcessReadWrite(CreatePinger(), webSocket, cancellationTokenSource);
                            await pinger.Merge(data).ForEachAsync(async task =>
                            {
                                try
                                {
                                    await task();
                                }
                                catch (WebSocketException)
                                {
                                    cancellationTokenSource.Cancel();
                                }
                                catch (TaskCanceledException) { }
                                catch (OperationCanceledException) { }
                                catch (InvalidOperationException ex) when (ex.Message == "Reading is not allowed after reader was completed.")
                                {
                                    cancellationTokenSource.Cancel();
                                }
                                catch (Exception ex)
                                {
                                    cancellationTokenSource.Cancel();
                                    logger.LogError(ex, "Failed websocket.");
                                }
                            }, cancellationTokenSource.Token);
                        }
                        catch (TaskCanceledException) { }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            logger.LogError(ex,"Failed websocket.");
                        }
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

        private static IObservable<Func<WebSocket, CancellationTokenSource, Task>> CreatePinger()
        {
            var answered = true;
            var interval = Observable.Interval(TimeSpan.FromMinutes(1))
                .Select<long, Func<WebSocket, CancellationTokenSource, Task>>(i =>
                {
                    return async (ws, cts) =>
                    {
                        if (!answered)
                        {
                            cts.Cancel();
                            return;
                        }

                        if (!ws.CloseStatus.HasValue && !cts.IsCancellationRequested)
                        {
                            await SendData(ws, cts, "ping");
                            answered = false;
                            if (!ws.CloseStatus.HasValue)
                            {
                                await ws.ReceiveAsync(new ArraySegment<byte>(new byte[2048]), cts.Token);
                                answered = true;
                            }
                            else
                                cts.Cancel();
                        }
                        else
                            cts.Cancel();
                    };
                });
            return interval;
        }

        private static IObservable<Func<Task>> ProcessReadWrite(IObservable<Func<WebSocket, CancellationTokenSource, Task>> observable, WebSocket webSocket, CancellationTokenSource cancellationTokenSource)
        {
            return observable.Select<Func<WebSocket, CancellationTokenSource, Task>, Func<Task>>(func => async () => { await func(webSocket, cancellationTokenSource); });
        }

        private static IObservable<Func<Task>> ProcessReadOnly(IObservable<object> observable, WebSocket webSocket, CancellationTokenSource cancellationTokenSource)
        {
            return observable.Select<object, Func<Task>>(data => async () => { await SendData(webSocket, cancellationTokenSource, data); });
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
