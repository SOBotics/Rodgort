using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbMetaTag
    {
        public const string STATUS_COMPLETED = "status-completed";
        public const string STATUS_PLANNED = "status-planned";
        public const string STATUS_DECLINED = "status-declined";

        public string Name { get; set; }

        public List<DbMetaQuestionMetaTag> MetaQuestionMetaTags { get; set; }
    }
}
