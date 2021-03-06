﻿using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbSiteUser
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public bool IsModerator { get; set; }
        public int Reputation { get; set; }

        public List<DbUserAction> UserActions { get; set; }
        public List<DbSiteUserRole> Roles { get; set; }
        public List<DbSiteUserRole> AddedRoles { get; set; }

        public List<DbUnknownDeletion> ProcessedUnknownDeletions { get; set; }

        public List<DbMetaQuestionTagTrackingStatusAudit> TagTrackingStatusAudits { get; set; }

        public List<DbSiteUserRoleAudit> UserRolesChanged { get; set; }
        public List<DbSiteUserRoleAudit> ChangedOtherRoles { get; set; }
    }
}
