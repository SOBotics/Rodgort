using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbMetaTag
    {
        public const string STATUS_COMPLETED = "status-completed";
        public const string STATUS_PLANNED = "status-planned";
        public const string STATUS_DECLINED = "status-declined";

        public const string STATUS_FEATURED = "featured";

        public static string[] StatusFlags = { STATUS_FEATURED, STATUS_COMPLETED, STATUS_DECLINED, STATUS_PLANNED };

        public const string BURNINATE_REQUEST = "burninate-request";
        public const string SYNONYM_REQUEST = "synonym-request";
        public const string RETAG_REQUEST = "retag-request";

        public static string[] RequestTypes = { BURNINATE_REQUEST, SYNONYM_REQUEST, RETAG_REQUEST };

        public string Name { get; set; }

        public List<DbMetaQuestionMetaTag> MetaQuestionMetaTags { get; set; }
    }
}
