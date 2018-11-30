using Newtonsoft.Json;

namespace StackExchangeChat
{
    public class ChatEvent
    {
        public EventDetails EventDetails { get; set; }
        public RoomDetails RoomDetails { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
