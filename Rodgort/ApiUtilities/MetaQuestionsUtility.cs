using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchangeApi;
using StackExchangeApi.Responses;

namespace Rodgort.ApiUtilities
{
    public static class MetaQuestionsUtility
    {
        private const string BaseQuestionFilter = "!1PVN1yfHP2mFv5xsYE3Iaa)w(-d*V(48A";

        public static Task<ApiItemsResponse<BaseQuestion>> MetaQuestionsByTag(this ApiClient apiClient, string siteName, string tag, PagingOptions pagingOptions = null)
        {
            return apiClient.ApplyWithPaging<BaseQuestion>($"{ApiClient.BASE_URL}/questions",
                new Dictionary<string, string>
                {
                    {"site", siteName},
                    {"tagged", tag},
                    {"filter", BaseQuestionFilter}
                }, pagingOptions);
        }

        public static Task<ApiItemsResponse<BaseQuestion>> MetaQuestionsByIds(this ApiClient apiClient, string siteName, List<int> questionIds, PagingOptions pagingOptions = null)
        {
            var questionIdString = string.Join(";", questionIds);
            return apiClient.ApplyWithPaging<BaseQuestion>($"{ApiClient.BASE_URL}/questions/{questionIdString}",
                new Dictionary<string, string>
                {
                    {"site", siteName},
                    {"filter", BaseQuestionFilter}
                }, pagingOptions);
        }
    }
}
