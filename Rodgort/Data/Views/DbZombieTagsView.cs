using System;
using Rodgort.Data.Tables;

namespace Rodgort.Data.Views
{
    public class DbZombieTagsView
    {
        public string TagName { get; set; }
        public DateTime TimeRevived { get; set; }

        public DbTag Tag { get; set; }
    }
}
