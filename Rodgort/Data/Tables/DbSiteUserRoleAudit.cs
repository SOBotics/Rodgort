using System;

namespace Rodgort.Data.Tables
{
    public class DbSiteUserRoleAudit
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ChangedByUserId { get; set; }

        public DateTime DateChanged { get; set; }

        public bool Added { get; set; }

        public DbSiteUser User { get; set; }
        public DbSiteUser ChangedByUser { get; set; }
        public DbSiteUserRole Role { get; set; }
    }
}
