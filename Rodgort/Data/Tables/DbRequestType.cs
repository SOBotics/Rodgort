using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbRequestType
    {
        public const int UNKNOWN = 0;
        public const int SYNONYM = 1;
        public const int MERGE = 2;
        public const int BURNINATE = 3;
        
        public int Id { get; set; }
        public string Name { get; set; }

        public List<DbMetaQuestionTag> MetaQuestionTags { get; set; }
    }
}
