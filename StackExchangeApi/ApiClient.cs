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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchangeApi.Responses;
using Utilities.Throttling;

namespace StackExchangeApi
{
    public class ApiClient
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApiClient> _logger;
        private readonly string _appKey;

        public static IObservable<int> QuotaRemaining;
        public static int CurrentQuotaRemaining = int.MaxValue;
        
        private static Action<int> _updateQuota;

        static ApiClient()
        {
            var replaySubject = new ReplaySubject<int>(1);
            Observable.Create<int>(o =>
            {
                _updateQuota = o.OnNext;
                return Disposable.Empty;
            }).Subscribe(replaySubject);

            QuotaRemaining = replaySubject;
            QuotaRemaining.Subscribe(remaining => { Interlocked.Exchange(ref CurrentQuotaRemaining, remaining); });
        }

        public ApiClient(IServiceProvider serviceProvider, IStackExchangeApiCredentials configuration, ILogger<ApiClient> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _appKey = configuration.AppKey;
        }

        public async Task<TResponseType> MakeRequest<TResponseType>(string endpoint, Dictionary<string, string> parameters) where TResponseType: ApiBaseResponse
        {
            var copiedParameters = parameters.ToDictionary(d => d.Key, d => d.Value);
            if (!string.IsNullOrWhiteSpace(_appKey) && !copiedParameters.ContainsKey("key"))
                copiedParameters["key"] = _appKey;

            var url = QueryHelpers.AddQueryString(endpoint, copiedParameters);

            return await ThrottlingUtils.Throttle(ApiThrottleGroups.ApiThrottleGroup, async () =>
            {
                if (CurrentQuotaRemaining <= 0)
                    throw new Exception("No more quota!");

                using (var httpClient = _serviceProvider.GetService<HttpClient>())
                {
                    var response = await httpClient.GetAsync(url);
                    var content = await response.Content.ReadAsStringAsync();
                    var payloadUntyped = JsonConvert.DeserializeObject<JObject>(content);
                    var payload = payloadUntyped.ToObject<TResponseType>();

                    payload.RawData = content;
                    payload.RequestUrl = url;

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
            }, async returnedItem =>
            {
                _updateQuota(returnedItem.QuotaRemaining);
                _logger.LogTrace($"Finished request {url}. Remaining quota: " + returnedItem.QuotaRemaining);
                if (returnedItem.Backoff.HasValue)
                    await Task.Delay(TimeSpan.FromSeconds(returnedItem.Backoff.Value));
            });
        }

        public const string BASE_URL = "https://api.stackexchange.com/2.2";

        public async Task<ApiItemsResponse<UserResponse>> Users(string siteName, IEnumerable<int> userIds)
        {
            ApiItemsResponse<UserResponse> finalResult = null;

            foreach (var batch in userIds.Batch(100))
            {
                var userIdsString = string.Join(";", batch);
                
                var currentResult = await ApplyWithPaging<UserResponse>($"{BASE_URL}/users/{userIdsString}", new Dictionary<string, string>
                {
                    {"site", siteName},
                    {"filter", "!JlNR05FuMA99pPFc(m7tLG4"}
                });
                if (finalResult == null)
                    finalResult = currentResult;
                else
                    finalResult.Items.AddRange(currentResult.Items);
            }

            return finalResult;
        }

        public async Task<ApiItemsResponse<ModeratorResponse>> Moderators(string siteName)
        {
            return await ApplyWithPaging<ModeratorResponse>($"{BASE_URL}/users/moderators",
                new Dictionary<string, string>
                {
                    {"site", siteName},
                    {"filter", "!qH8bjUGDbuo.sdwpgswz"}
                });
        }

        public Task<ApiItemsResponse<TagSynonymsResponse>> TagSynonyms(string siteName)
        {
            return ApplyWithPaging<TagSynonymsResponse>($"{BASE_URL}/tags/synonyms", new Dictionary<string, string>
            {
                {"site", siteName},
                {"filter", "!--o_SwqiUDuP"},
            });
        }

        public Task<ApiItemsResponse<RevisionResponse>> Revisions(string siteName, IEnumerable<int> postIds)
        {
            var postIdsList = postIds.ToList();
            var postIdsString = string.Join(";", postIdsList);
            return ApplyWithPaging<RevisionResponse>($"{BASE_URL}/posts/{postIdsString}/revisions", new Dictionary<string, string>
            {
                {"site", siteName},
                {"filter", "!FcbKgREm*513I8Z7LLLeTy.2SW"},
            });
        }

        public Task<ApiItemsResponse<QuestionIdResponse>> QuestionsByTag(string siteName, string tag)
        {
            return ApplyWithPaging<QuestionIdResponse>($"{BASE_URL}/questions?tagged={tag}", new Dictionary<string, string>
            {
                {"site", siteName},
                {"filter", "!-W2eZXqTF)pIrxsccZvx"}
            });
        }

        public Task<ApiItemsResponse<TagResponse>> TotalQuestionsByTag(string siteName, IEnumerable<string> tags)
        {
            var tagsList = tags.ToList();
            var tagString = string.Join(";", tagsList.Select(HttpUtility.UrlEncode));
            return MakeRequest<ApiItemsResponse<TagResponse>>($"{BASE_URL}/tags/{tagString}/info", new Dictionary<string, string>
            {
                {"site", siteName},
                {"filter", "!9Z(-wqiNh"},
                { "pageSize", tagsList.Count.ToString() }
            });
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
                    page++;
                    copiedParameters["page"] = page.ToString();
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
