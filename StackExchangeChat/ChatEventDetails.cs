using Newtonsoft.Json;

namespace StackExchangeChat
{
    public class ChatEventDetails
    {
        [JsonProperty(PropertyName = "event_type")]
        public ChatEventType ChatEventType { get; set; } 

        [JsonProperty(PropertyName = "time_stamp")]
        public int TimeStamp { get; set; }

        public string Content { get; set; }

        public int Id { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "user_name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "room_id")]
        public int RoomId { get; set; }

        [JsonProperty(PropertyName = "room_name")]
        public string RoomName { get; set; }

        [JsonProperty(PropertyName = "message_id")]
        public int? MessageId { get; set; }
    }
}
