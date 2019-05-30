using Rodgort.Data.Tables;

namespace Rodgort.Data.Views
{
    public class DbUserStatisticsView
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public bool IsModerator { get; set; }
        public int NumBurnActions { get; set; }
        public int TriagedTags { get; set; }
        public int TriagedQuestions { get; set; }

        public DbSiteUser SiteUser { get; set; }
    }
}
