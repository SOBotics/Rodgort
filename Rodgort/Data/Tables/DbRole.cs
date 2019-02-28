using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbRole
    {
        public const int RODGORT_SUPER_USER = 1;
        public const int RODGORT_ADMIN = 2;
        
        public int Id { get; set; }

        public string Name { get; set; }

        public List<DbSiteUserRole> SiteUserRoles { get; set; }
    }
}
