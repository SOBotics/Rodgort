using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchangeApi;
using StackExchangeApi.Responses;

namespace Rodgort.ApiUtilities
{
    public static class MetaQuestionsUtility
    {
        public static Task<ApiItemsResponse<BaseQuestion>> MetaQuestionsByTag(this ApiClient apiClient, string siteName, string tag, PagingOptions pagingOptions = null)
        {
            return apiClient.ApplyWithPaging<BaseQuestion>($"{ApiClient.BASE_URL}/questions",
                new Dictionary<string, string>
                {
                    {"site", siteName},
                    {"tagged", tag},
                    {"filter", "!1PVL*Tfn3U8E_DNqhT*s*BC)u-eMGS1)f"}
                }, pagingOptions);
        }
    }
}
