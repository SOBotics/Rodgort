using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using StackExchangeApi;

namespace Rodgort.Services
{
    public static class QuotaRemainingWebsocketService
    {
        public static void ConfigureQuotaRemainingWebsocket(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws/quotaRemaining")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var cancellationTokenSource = new CancellationTokenSource();

                        // We're using ForEachAsync rather than Subscribe because I was unable to get reactivex to execute the observer code
                        // On a context in which the websocket hadn't been closed. 
                        await ApiClient.QuotaRemaining.ForEachAsync(async quotaRemaining =>
                        {
                            var payload = new {quotaRemaining};
                            var payloadStr = JsonConvert.SerializeObject(payload);
                            var payloadBytes = Encoding.UTF8.GetBytes(payloadStr);
                            var bytes = new ArraySegment<byte>(payloadBytes);
                            if (!webSocket.CloseStatus.HasValue)
                            {
                                try
                                {
                                    await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true,
                                        CancellationToken.None);
                                }
                                catch (Exception)
                                {
                                    cancellationTokenSource.Cancel();
                                }
                            }
                            else
                                cancellationTokenSource.Cancel();
                        }, cancellationTokenSource.Token);
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
    }
}
