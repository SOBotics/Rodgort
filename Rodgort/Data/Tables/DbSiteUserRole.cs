﻿using System;
using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbSiteUserRole
    {
        public int UserId { get; set; }
        public string RoleName { get; set; }

        public int AddedByUserId { get; set; }
        public DateTime DateAdded { get; set; }
        public bool Enabled { get; set; }

        public DbSiteUser User { get; set; }
        public DbSiteUser AddedByUser { get; set; }
        public DbRole Role { get; set; }

        public List<DbSiteUserRoleAudit> Audits { get; set; }
    }
}
