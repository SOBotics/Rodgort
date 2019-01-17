using System;

namespace Rodgort.Data.Tables
{
    public class DbUserAction
    {
        public int Id { get; set; }

        public int SiteUserId { get; set; }

        public int PostId { get; set; }

        public string Tag { get; set; }

        public DateTime Time { get; set; }

        public int UserActionTypeId { get; set; }

        public DateTime TimeProcessed { get; set; }

        public DbUserActionType UserActionType { get; set; }
        public DbSiteUser SiteUser { get; set; }
    }
}
