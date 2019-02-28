using Newtonsoft.Json;

namespace StackExchangeApi.Responses
{
    public class UserResponse
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("user_type")]
        public string UserType { get; set; }

        public int Reputation { get; set; }
    }
}
