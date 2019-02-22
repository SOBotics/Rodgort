using System;

namespace Rodgort.Data.Tables
{
    public class DbTagStatistics
    {
        public int Id { get; set; }

        public string TagName { get; set; }

        public int QuestionCount { get; set; }

        public int AnswerCount { get; set; }

        public bool IsSynonym { get; set; }

        public DateTime DateTime { get; set; }
        
        public DbTag Tag { get; set; }
    }
}
