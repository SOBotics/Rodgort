using System;
using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbMetaQuestion
    {
        public int Id { get; set; }

        public string Body { get; set; }

        public string Title { get; set; }

        public string Link { get; set; }

        public int Score { get; set; }

        public int ViewCount { get; set; }

        public string CloseReason { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public DateTime? FeaturedStarted { get; set; }
        public DateTime? FeaturedEnded { get; set; }

        public DateTime? BurnStarted { get; set; }
        public DateTime? BurnEnded { get; set; }

        public DateTime LastSeen { get; set; }
        
        public List<DbMetaAnswer> MetaAnswers { get; set; } = new List<DbMetaAnswer>();
        public List<DbMetaQuestionMetaTag> MetaQuestionMetaTags { get; set; } = new List<DbMetaQuestionMetaTag>();
        public List<DbMetaQuestionTag> MetaQuestionTags { get; set; } = new List<DbMetaQuestionTag>();
        public List<DbMetaQuestionStatistics> Statistics { get; set; } = new List<DbMetaQuestionStatistics>();
        public List<DbMetaQuestionTagTrackingStatusAudit> TagTrackingStatusAudits { get; set; }
    }
}
