using System;

namespace Rodgort.Data.Tables
{
    public class DbBurnakiFollow
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int BurnakiId { get; set; }

        public DateTime FollowStarted { get; set; }
        public DateTime? FollowEnded { get; set; }
    }
}
