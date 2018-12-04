using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbMetaQuestion
    {
        public int Id { get; set; }

        public string Body { get; set; }

        public string Title { get; set; }

        public string Link { get; set; }

        public List<DbMetaAnswer> MetaAnswers { get; set; }
        public List<DbMetaQuestionTag> MetaQuestionTags { get; set; }
        public List<DbMetaQuestionStatistics> Statistics { get; set; }
    }
}
