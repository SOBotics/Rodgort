using Newtonsoft.Json;

namespace StackExchangeApi.Responses
{
    public class ModeratorResponse
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }
    }
}
