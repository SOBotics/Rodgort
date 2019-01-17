using System;
using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbUnknownDeletion
    {
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public DateTime? Processed { get; set; }

        public int PostId { get; set; }

        public string Tag { get; set; }

        public int? ProcessedByUserId { get; set; }

        public DbSiteUser ProcessedByUser { get; set; }
        public List<DbUserAction> UserActions { get; set; }
    }
}
