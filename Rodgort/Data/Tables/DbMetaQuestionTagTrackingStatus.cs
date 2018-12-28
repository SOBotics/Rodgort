using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbMetaQuestionTagTrackingStatus
    {
        public const int REQUIRES_TRACKING_APPROVAL = 1;
        public const int TRACKED = 2;
        public const int IGNORED = 3;
        public const int TRACKED_ELSEWHERE = 4;

        public int Id { get; set; }

        public string Name { get; set; }

        public List<DbMetaQuestionTag> MetaQuestionTags { get; set; }
    }
}
