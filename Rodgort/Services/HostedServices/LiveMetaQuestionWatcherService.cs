using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rodgort.ApiUtilities;
using Rodgort.Utilities.ReactiveX;
using StackExchangeApi;
using StackExchangeChat;

namespace Rodgort.Services.HostedServices
{
    public class LiveMetaQuestionWatcherService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BurnakiFollowService> _logger;

        public LiveMetaQuestionWatcherService(IServiceProvider serviceProvider, ILogger<BurnakiFollowService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var websocket = CreateLiveWebsocket();
                websocket
                    .SlidingBuffer(TimeSpan.FromSeconds(5))
                    .Subscribe(async questionIdList =>
                    {
                        foreach (var batch in questionIdList.Distinct().Batch(95))
                        {
                            try
                            {
                                var batchList = batch.ToList();
                                _logger.LogInformation($"Processing question(s) {string.Join(",", batchList)} from meta websocket");

                                using (var scope = _serviceProvider.CreateScope())
                                {
                                    var metaCrawlerService = scope.ServiceProvider.GetRequiredService<MetaCrawlerService>();
                                    var apiClient = scope.ServiceProvider.GetRequiredService<ApiClient>();
                                    var questions = await apiClient.MetaQuestionsByIds("meta.stackoverflow.com", batchList.ToList());
                                    var result = metaCrawlerService.ProcessQuestions(questions.Items);
                                    await metaCrawlerService.PostProcessQuestions(questions.Items, result);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("Failed processing meta websocket batch", ex);
                            }
                        }
                    }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed watching live meta", ex);
            }
            return Task.CompletedTask;
        }

        private IObservable<int> CreateLiveWebsocket()
        {
            const string wsEndpoint = "wss://qa.sockets.stackexchange.com/";
            const string homePage = "https://stackoverflow.com";

            var websocket = Observable.Create<int>(async observer =>
            {
                var webSocket = new PlainWebSocket(wsEndpoint, new Dictionary<string, string> {{"Origin", homePage}}, _serviceProvider.GetRequiredService<ILogger<PlainWebSocket>>());
                webSocket.OnTextMessage += async message =>
                {
                    try
                    {
                        var messageObject = JsonConvert.DeserializeObject<JObject>(message);
                        var dataStr = messageObject["data"].Value<string>();
                        if (string.Equals(dataStr, "pong"))
                        {
                            await webSocket.SendAsync("pong");
                            return;
                        }

                        var payload = JsonConvert.DeserializeObject<JObject>(dataStr);
                        
                        var questionId = payload.First.First.Value<int>();

                        observer.OnNext(questionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to process 552-home-active for 'https://stackoverflow.com'. Message received: " + message, ex);
                    }
                };

                try
                {
                    await webSocket.ConnectAsync();
                    await webSocket.SendAsync("552-home-active");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to start websocket", ex);
                }

                _logger.LogTrace("Connected to 552-home-active on meta.stackoverflow.com");

                return Disposable.Create(() =>
                {
                    _logger.LogWarning("Disposing meta live websocket");
                    webSocket?.Dispose();
                });
            });
            return websocket.Publish().RefCount();
        }
    }
}
