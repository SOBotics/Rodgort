using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbSiteUser
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public List<DbUserAction> UserActions { get; set; }
        public List<DbSiteUserRole> Roles { get; set; }
        public List<DbSiteUserRole> AddedRoles { get; set; }

        public List<DbUnknownDeletion> ProcessedUnknownDeletions { get; set; }

        public List<DbMetaQuestionTagTrackingStatusAudit> TagTrackingStatusAudits { get; set; }
    }
}
