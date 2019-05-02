namespace Rodgort.Data.Tables
{
    public class DbPinnedMessages
    {
        public int Id { get; set; }

        public string ChatDomain { get; set; }
        public int MessageId { get; set; }
        public int RoomId { get; set; }
    }
}
