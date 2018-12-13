using System;

namespace Rodgort.Data.Tables
{
    public class DbLog
    {
        public int Id { get; set; }
        public DateTime TimeLogged { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string Logger { get; set; }
        public string Callsite { get; set; }
    }
}
