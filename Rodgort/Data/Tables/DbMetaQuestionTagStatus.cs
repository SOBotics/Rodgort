using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbMetaQuestionTagStatus
    {
        public const int GUESSED = 1;
        public const int APPROVED = 2;
        public const int DECLINED = 3;
        
        public int Id { get; set; }

        public string Name { get; set; }

        public List<DbMetaQuestionTag> MetaQuestionTags { get; set; }
    }
}
