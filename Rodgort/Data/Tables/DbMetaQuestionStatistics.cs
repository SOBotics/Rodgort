using System;

namespace Rodgort.Data.Tables
{
    public class DbMetaQuestionStatistics
    {
        public int Id { get; set; }

        public int MetaQuestionId { get; set; }
       
        public int Score { get; set; }

        public int ViewCount { get; set; }

        public DateTime DateTime { get; set; }

        public DbMetaQuestion MetaQuestion { get; set; }
    }
}
