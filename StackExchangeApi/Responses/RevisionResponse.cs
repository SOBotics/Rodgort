using System.Collections.Generic;
using Newtonsoft.Json;

namespace StackExchangeApi.Responses
{
    public class RevisionResponse
    {
        [JsonProperty("last_tags")]
        public List<string> LastTags { get; set; }

        public List<string> Tags { get; set; }

        [JsonProperty("creation_date")]
        public int CreationDate { get; set; }

        public RevisionUserResponse User { get; set; }
    }

    public class RevisionUserResponse
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }
    }
}
