
using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbTag
    {
        public string Name { get; set; }

        public int? NumberOfQuestions { get; set; }

        public List<DbMetaQuestionTag> MetaQuestionTags { get; set; }
        public List<DbMetaQuestionTag> MetaQuestionSecondaryTags { get; set; }
        public List<DbTagStatistics> Statistics { get; set; }
    }
}
