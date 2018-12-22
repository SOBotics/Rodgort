namespace Rodgort.Data.Tables
{
    public class DbSiteUserRole
    {
        public int UserId { get; set; }
        public string RoleName { get; set; }

        public DbSiteUser User { get; set; }
        public DbRole Role { get; set; }
    }
}
