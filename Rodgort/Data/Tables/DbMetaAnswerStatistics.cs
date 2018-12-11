using System;

namespace Rodgort.Data.Tables
{
    public class DbMetaAnswerStatistics
    {
        public int Id { get; set; }

        public int MetaAnswerId { get; set; }
        
        public int Score { get; set; }

        public DateTime DateTime { get; set; }
        
        public DbMetaAnswer MetaAnswer { get; set; }
    }
}
