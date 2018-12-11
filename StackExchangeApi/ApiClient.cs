using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchangeApi.Responses;

namespace StackExchangeApi
{
    public class ApiClient
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _accessToken;
        private readonly string _appKey;

        public static IObservable<int> QuotaRemaining;
        public static int CurrentQuotaRemaining = int.MaxValue;
        
        public static object TaskLocker = new object();
        public static Task ExecutingTask = Task.CompletedTask;

        private static Action<int> m_UpdateQuota;

        static ApiClient()
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

        public ApiClient(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _accessToken = configuration.GetSection("AccessToken").Value;
            _appKey = configuration.GetSection("AppKey").Value;
        }

        public async Task<TResponseType> MakeRequest<TResponseType>(string endpoint, Dictionary<string, string> parameters) where TResponseType: ApiBaseResponse
        {
            Task<TResponseType> nextTask;
            var copiedParameters = parameters.ToDictionary(d => d.Key, d => d.Value);
            if (!string.IsNullOrWhiteSpace(_accessToken) && !copiedParameters.ContainsKey("access_token"))
                copiedParameters["access_token"] = _accessToken;

            if (!string.IsNullOrWhiteSpace(_appKey) && !copiedParameters.ContainsKey("key"))
                copiedParameters["key"] = _appKey;

            var url = QueryHelpers.AddQueryString(endpoint, copiedParameters);
            lock (TaskLocker)
            {
                if (ExecutingTask.IsFaulted)
                    ExecutingTask = Task.CompletedTask;
                
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
                    var response = await httpClient.GetAsync(url);
                    var content = await response.Content.ReadAsStringAsync();
                    var payloadUntyped = JsonConvert.DeserializeObject<JObject>(content);
                    var payload = payloadUntyped.ToObject<TResponseType>();

                    if (!string.IsNullOrWhiteSpace(payload.ErrorMessage))
                        throw new Exception($"Failed to request {url}.\n\n" + JsonConvert.SerializeObject(new
                        {
                            payload.ErrorId,
                            payload.ErrorName,
                            payload.ErrorMessage,
                        }));

                    var quotaRemaining = payloadUntyped["quota_remaining"];
                    if (quotaRemaining == null)
                        throw new Exception("Response did not fail, but did not include quota_remaining. Please ensure filter returns this field.");

                    return payload;
                }
            }
        }

        public const string BASE_URL = "https://api.stackexchange.com/2.2";

        public Task<TotalResponse> TotalQuestionsByTag(string siteName, string tag)
        {
            return MakeRequest<TotalResponse>($"{BASE_URL}/questions", new Dictionary<string, string>
            {
                { "site", siteName },
                { "tagged", tag },
                { "filter", "!--s3oyShP3gx" }
            });
        }

        public Task<ApiItemsResponse<BaseQuestion>> QuestionsByTag(string siteName, string tag, PagingOptions pagingOptions = null)
        {
            return ApplyWithPaging<BaseQuestion>($"{BASE_URL}/questions", new Dictionary<string, string>
            {
                { "site", siteName },
                { "tagged", tag },
                { "filter", "!bHIU4eJgUSOOHK" }
            }, pagingOptions);
        }


        public async Task<ApiItemsResponse<TItemType>> ApplyWithPaging<TItemType>(
            string endPoint,
            Dictionary<string, string> parameters, 
            PagingOptions pagingOptions = null)
        {
            if (pagingOptions == null)
                pagingOptions = new PagingOptions();

            var copiedParameters = parameters.ToDictionary(d => d.Key, d => d.Value);
            var runningItems = new List<TItemType>();
            var page = pagingOptions.Page;

            copiedParameters["page"] = pagingOptions.Page.ToString();
            copiedParameters["pageSize"] = pagingOptions.PageSize.ToString();

            var result = await MakeRequest<ApiItemsResponse<TItemType>>(endPoint, copiedParameters);
            if (pagingOptions.AutoFetchAll)
            {
                while (result.HasMore)
                {
                    copiedParameters["page"] = page++.ToString();
                    runningItems.AddRange(result.Items);
                    result = await MakeRequest<ApiItemsResponse<TItemType>>(endPoint, copiedParameters);
                }

                runningItems.AddRange(result.Items);
                result.Items = runningItems;
            }

            return result;
        }
    }
}
