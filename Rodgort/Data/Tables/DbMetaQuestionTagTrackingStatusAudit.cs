using System;

namespace Rodgort.Data.Tables
{
    public class DbMetaQuestionTagTrackingStatusAudit
    {
        public int Id { get; set; }

        public DateTime TimeChanged { get; set; }

        public int? PreviousTrackingStatusId { get; set; }
        public int NewTrackingStatusId { get; set; }

        public int ChangedByUserId { get; set; }

        public int MetaQuestionId { get; set; }
        public string Tag { get; set; }

        public DbMetaQuestionTagTrackingStatus PreviousTrackingStatus { get; set; }
        public DbMetaQuestionTagTrackingStatus NewTrackingStatus { get; set; }

        public DbMetaQuestion MetaQuestion { get; set; }

        public DbMetaQuestionTag MetaQuestionTag { get; set; }
        public DbSiteUser ChangedByUser { get; set; }
    }
}
