using System;

namespace Rodgort.Data.Tables
{
    public class DbSeenQuestion
    {
        public int Id { get; set; }
        public string Tag { get; set; }

        public DateTime LastSeen { get; set; }
    }
}
