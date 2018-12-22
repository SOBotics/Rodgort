using Newtonsoft.Json;

namespace StackExchangeChat
{
    public class ChatEvent
    {
        public ChatClient ChatClient { get; set; }

        public ChatEventDetails ChatEventDetails { get; set; }
        public RoomDetails RoomDetails { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
