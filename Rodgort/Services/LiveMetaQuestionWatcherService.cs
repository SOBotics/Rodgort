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

namespace Rodgort.Services
{
    public class LiveMetaQuestionWatcherService : IHostedService
    {
        private readonly MetaCrawlerService _metaCrawlerService;
        private readonly ApiClient _apiClient;
        private readonly ILogger<BurnakiFollowService> _logger;

        public LiveMetaQuestionWatcherService(IServiceProvider serviceProvider, ILogger<BurnakiFollowService> logger)
        {
            var scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;
            _metaCrawlerService = scopedServiceProvider.GetRequiredService<MetaCrawlerService>();
            _apiClient = scopedServiceProvider.GetRequiredService<ApiClient>();
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var websocket = CreateLiveWebsocket();

            websocket
                .SlidingBuffer(TimeSpan.FromSeconds(5))
                .Subscribe(async questionIdList =>
            {
                foreach (var batch in questionIdList.Distinct().Batch(95))
                {
                    var questions = await _apiClient.MetaQuestionsByIds("meta.stackoverflow.com", batch.ToList());
                    var result = _metaCrawlerService.ProcessQuestions(questions.Items);
                    await _metaCrawlerService.PostProcessQuestions(result);
                }
            });

            return Task.CompletedTask;
        }

        private IObservable<int> CreateLiveWebsocket()
        {
            const string wsEndpoint = "wss://qa.sockets.stackexchange.com/";
            const string homePage = "https://stackoverflow.com";

            var websocket = Observable.Create<int>(async observer =>
            {
                var webSocket = new PlainWebSocket(wsEndpoint, new Dictionary<string, string> {{"Origin", homePage}});
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

                await webSocket.ConnectAsync();
                await webSocket.SendAsync("552-home-active");

                return Disposable.Empty;
            });
            return websocket;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
