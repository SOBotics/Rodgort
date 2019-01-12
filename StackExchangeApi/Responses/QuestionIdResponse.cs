using System.Collections.Generic;
using Newtonsoft.Json;

namespace StackExchangeApi.Responses
{
    public class QuestionIdResponse
    {
        [JsonProperty("question_id")]
        public int QuestionId { get; set; }

        public List<string> Tags { get; set; }
    }
}
