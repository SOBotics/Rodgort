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

                                    var result = metaCrawlerService.ProcessQuestions(questions.Items, false);
                                    await metaCrawlerService.PostProcessQuestions(questions.Items, result);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed processing meta websocket batch");
                            }
                        }
                    }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed watching live meta");
            }
            return Task.CompletedTask;
        }

        private IObservable<int> CreateLiveWebsocket()
        {
            const string wsEndpoint = "wss://qa.sockets.stackexchange.com/";
            const string homePage = "https://stackoverflow.com";

            var websocket = Observable.Create<int>(async observer =>
            {
                var scope = _serviceProvider.CreateScope();
                var connection = await scope.ServiceProvider.GetService<ObservableClientWebSocket>().Connect(wsEndpoint, new Dictionary<string, string> {{"Origin", homePage}});

                await connection.Send("552-home-active");

                var messages = connection.Messages().Select(data =>
                {
                    var messageObject = JsonConvert.DeserializeObject<JObject>(data);
                    return messageObject["data"].Value<string>();
                });

                messages.Where(dataStr => string.Equals(dataStr, "pong")).Subscribe(async _ => await connection.Send("pong"));
                messages.Where(dataStr => !string.Equals(dataStr, "pong")).Select(dataStr =>
                    {
                        var payload = JsonConvert.DeserializeObject<JObject>(dataStr);

                        return payload.First.First.Value<int>();
                    })
                    .Subscribe(observer);

                return Disposable.Empty;
            });
            return websocket.Publish().RefCount();
        }
    }
}
