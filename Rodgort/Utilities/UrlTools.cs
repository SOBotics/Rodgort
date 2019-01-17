using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rodgort.Utilities
{
    public class UrlTools
    {
        public static string BuildUrl(string url, Dictionary<string, string> queryArgs)
        {
            if (!queryArgs.Any())
                return url;

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var queryArg in queryArgs)
                queryString.Add(queryArg.Key, queryArg.Value);

            return $"{url}?{queryString}";
        }
    }
}
