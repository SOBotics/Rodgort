using Newtonsoft.Json;

namespace StackExchangeChat
{
    public class ChatEvent
    {
        public ChatEventDetails ChatEventDetails { get; set; }
        public RoomDetails RoomDetails { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
