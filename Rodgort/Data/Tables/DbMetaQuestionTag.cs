using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbMetaQuestionTag
    {
        public int MetaQuestionId { get; set; }
        public string TagName { get; set; }
        public int TrackingStatusId { get; set; }

        public string SecondaryTagName { get; set; }

        public DbMetaQuestion MetaQuestion { get; set; }
        public DbTag Tag { get; set; }
        public DbMetaQuestionTagTrackingStatus TrackingStatus { get; set; }
        public DbTag SecondaryTag { get; set; }

        public List<DbMetaQuestionTagTrackingStatusAudit> TagTrackingStatusAudits { get; set; }
    }
}
