using System.Collections.Generic;
using Newtonsoft.Json;

namespace StackExchangeApi.Responses
{
    public class TagResponse
    {
        public List<string> Synonyms { get; set; }

        [JsonProperty("last_activity_date")]
        public int LastActivityDate { get; set; }

        [JsonProperty("has_synonyms")]
        public bool? HasSynonyms { get; set; }

        [JsonProperty("is_moderator_only")]
        public bool? IsModeratorOnly { get; set; }

        [JsonProperty("is_required")]
        public bool? IsRequired { get; set; }

        public int Count { get; set; }
        public string Name { get; set; }
    }
}
