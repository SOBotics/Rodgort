using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbRole
    {
        public const int TRIAGER = 1;
        public const int ADMIN = 2;
        public const int TRUSTED = 3;

        public int Id { get; set; }

        public string Name { get; set; }

        public List<DbSiteUserRole> SiteUserRoles { get; set; }
    }
}
