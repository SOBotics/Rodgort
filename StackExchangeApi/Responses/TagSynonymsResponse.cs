using Newtonsoft.Json;

namespace StackExchangeApi.Responses
{
    public class TagSynonymsResponse
    {
        [JsonProperty("from_tag")]
        public string FromTag { get; set; }

        [JsonProperty("to_tag")]
        public string ToTag { get; set; }
    }
}
