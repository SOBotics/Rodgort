using System;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchangeChat.Utilities.Responses;

namespace StackExchangeChat.Utilities
{
    public class StackExchangeApiHelper
    {
        private readonly IServiceProvider _serviceProvider;

        public static IObservable<int> QuotaRemaining;
        public static int CurrentQuotaRemaining = int.MaxValue;
        
        public static object TaskLocker = new object();
        public static Task ExecutingTask = Task.CompletedTask;

        private static Action<int> m_UpdateQuota;

        static StackExchangeApiHelper()
        {
            var replaySubject = new ReplaySubject<int>(1);
            Observable.Create<int>(o =>
            {
                m_UpdateQuota = o.OnNext;
                return Disposable.Empty;
            }).Subscribe(replaySubject);

            QuotaRemaining = replaySubject;
            QuotaRemaining.Subscribe(remaining => { Interlocked.Exchange(ref CurrentQuotaRemaining, remaining); });
        }

        public StackExchangeApiHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponseType> MakeRequest<TResponseType>(string endpoint) where TResponseType: ApiBaseResponse
        {
            Task<TResponseType> nextTask;
            
            lock (TaskLocker)
            {
                nextTask = MakeRequestInternal(ExecutingTask);
                ExecutingTask = PostProcess(nextTask);
            }

            var result = await nextTask;            
            return result;

            async Task PostProcess(Task<TResponseType> executingTask)
            {
                var returnedItem = await executingTask;
                m_UpdateQuota(returnedItem.QuotaRemaining);
                if (returnedItem.Backoff.HasValue)
                    await Task.Delay(TimeSpan.FromSeconds(returnedItem.Backoff.Value));
            }

            async Task<TResponseType> MakeRequestInternal(Task previousTask)
            {
                await previousTask;
                if (CurrentQuotaRemaining <= 0)
                    throw new Exception("No more quota!");

                using (var httpClient = _serviceProvider.GetService<HttpClient>())
                {
                    var response = await httpClient.GetAsync(endpoint);
                    var content = await response.Content.ReadAsStringAsync();
                    var payload = JsonConvert.DeserializeObject<TResponseType>(content);
                    return payload;
                }
            }
        }

        public Task<TotalResponse> TotalQuestionsByTag(string tag)
        {
            var encodedTag = HttpUtility.HtmlEncode(tag);
            var query = $"https://api.stackexchange.com/2.2/questions?order=desc&sort=activity&tagged={encodedTag}&site=stackoverflow&filter=!--s3oyShP3gx";
            return MakeRequest<TotalResponse>(query);
        }
    }
}
