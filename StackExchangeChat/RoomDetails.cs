namespace StackExchangeChat
{
    public class RoomDetails
    {
        public ChatSite ChatSite { get; set; }

        /// <summary>
        /// Note that this is the room id of the room being subscribed to.
        /// However, pings from other rooms will still trigger events for this room. This means this might not match the event's room id.
        /// </summary>
        public int RoomId { get; set; }

        public int MyUserId { get; set; }
        public string MyUserName { get; set; }

        public string FKey { get; set; }
    }
}
