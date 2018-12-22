using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbRole
    {
        public const string TROGDOR_ROOM_OWNER = "Trogdor Room Owner";
        public const string RODGORT_ADMIN = "Rodgort Admin";
        public const string MODERATOR = "Moderator";

        public string Name { get; set; }

        public List<DbSiteUserRole> SiteUserRoles { get; set; }
    }
}
