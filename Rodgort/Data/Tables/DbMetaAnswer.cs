using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbMetaAnswer
    {
        public int Id { get; set; }

        public int MetaQuestionId { get; set; }

        public string Body { get; set; }

        public DbMetaQuestion MetaQuestion { get; set; }

        public List<DbMetaAnswerStatistics> Statistics { get; set; }
    }
}
