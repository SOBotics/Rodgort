using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbSiteUser
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public List<DbUserAction> UserActions { get; set; }
    }
}
