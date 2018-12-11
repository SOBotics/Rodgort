using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbRequestType
    {
        public const int UNKNOWN = 1;
        public const int SYNONYM = 2;
        public const int MERGE = 3;
        public const int BURNINATE = 4;
        
        public int Id { get; set; }
        public string Name { get; set; }

        public List<DbMetaQuestionTag> MetaQuestionTags { get; set; }
    }
}
