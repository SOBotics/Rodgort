using System;

namespace Rodgort.Data.Tables
{
    public class DbUserRetag
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Tag { get; set; }

        public bool Removed { get; set; }

        public DateTime Time { get; set; }
    }
}
