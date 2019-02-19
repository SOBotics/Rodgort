using Rodgort.Data.Tables;

namespace Rodgort.Data.Views
{
    public class DbViewPostState
    {
        public int UserActionId { get; set; }
        public int PostId { get; set; }
        public string Tag { get; set; }
        public bool Closed { get; set; }
        public bool Deleted { get; set; }
        public bool RemovedTag { get; set; }
        public bool Roombad { get; set; }

        public DbUserAction UserAction { get; set; }
    }
}
