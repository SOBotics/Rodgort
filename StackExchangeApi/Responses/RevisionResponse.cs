using System.Collections.Generic;
using Newtonsoft.Json;

namespace StackExchangeApi.Responses
{
    public class RevisionResponse
    {
        [JsonProperty("revision_number")]
        public string RevisionNumber { get; set; }

        [JsonProperty("post_id")]
        public int PostId { get; set; }

        [JsonProperty("last_tags")]
        public List<string> LastTags { get; set; }

        public List<string> Tags { get; set; }

        [JsonProperty("last_body")]
        public string LastBody { get; set; }

        [JsonProperty("last_title")]
        public string LastTitle { get; set; }

        public string Comment { get; set; }

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
