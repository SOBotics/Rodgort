using Newtonsoft.Json;

namespace StackExchangeChat.Api.Responses
{
    public class ApiBaseResponse
    {
        public int? Backoff { get; set; }

        [JsonProperty("quota_remaining")]
        public int QuotaRemaining { get; set; }

        [JsonProperty("error_id")]
        public int? ErrorId { get; set; }
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }
        [JsonProperty("error_name")]
        public string ErrorName { get; set; }
    }
}
