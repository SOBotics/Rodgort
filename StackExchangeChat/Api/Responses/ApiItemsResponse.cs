using System.Collections.Generic;
using Newtonsoft.Json;

namespace StackExchangeChat.Api.Responses
{
    public class ApiItemsResponse<TItemType> : ApiBaseResponse
    {
        public int? Page { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }
        public List<TItemType> Items { get; set; }
    }
}
