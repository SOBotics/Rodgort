using Microsoft.EntityFrameworkCore;
using Rodgort.Data.Tables;

namespace Rodgort.Data
{
    public class RodgortContext : DbContext
    {
        public RodgortContext(DbContextOptions<RodgortContext> options) : base(options)
        {
        }

        public DbSet<DbMetaQuestion> MetaQuestions { get; set; }
        public DbSet<DbMetaAnswer> MetaAnswers { get; set; }
        public DbSet<DbMetaQuestionTag> MetaQuestionTags { get; set; }
        public DbSet<DbMetaQuestionMetaTag> MetaQuestionMetaTags { get; set; }
        public DbSet<DbTag> Tags { get; set; }
        public DbSet<DbMetaTag> MetaTags { get; set; }
        public DbSet<DbTagStatistics> TagStatistics { get; set; }
        public DbSet<DbLog> Logs { get; set; }
        public DbSet<DbBurnakiFollow> BurnakiFollows { get; set; }
        public DbSet<DbUserAction> UserActions { get; set; }
        public DbSet<DbSiteUser> SiteUsers { get; set; }
        public DbSet<DbSiteUserRole> SiteUserRoles { get; set; }
        public DbSet<DbUnknownDeletion> UnknownDeletions { get; set; }
        public DbSet<DbSeenQuestion> SeenQuestions { get; set; }
        public DbSet<DbMetaQuestionTagTrackingStatusAudit> MetaQuestionTagTrackingStatusAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DbMetaAnswer>().ToTable("MetaAnswers");
            modelBuilder.Entity<DbMetaAnswer>().HasKey(ma => ma.Id);
            modelBuilder.Entity<DbMetaAnswer>().Property(rt => rt.Id).ValueGeneratedNever();
            modelBuilder.Entity<DbMetaAnswer>().HasOne(ma => ma.MetaQuestion).WithMany(mq => mq.MetaAnswers).HasForeignKey(ma => ma.MetaQuestionId);

            modelBuilder.Entity<DbMetaAnswerStatistics>().ToTable("MetaAnswerStatistics");
            modelBuilder.Entity<DbMetaAnswerStatistics>().HasKey(mas => mas.Id);
            modelBuilder.Entity<DbMetaAnswerStatistics>().HasOne(mas => mas.MetaAnswer).WithMany(ma => ma.Statistics).HasForeignKey(mas => mas.MetaAnswerId);

            modelBuilder.Entity<DbMetaQuestion>().ToTable("MetaQuestions");
            modelBuilder.Entity<DbMetaQuestion>().HasKey(mqs => mqs.Id);
            modelBuilder.Entity<DbMetaQuestion>().Property(rt => rt.Id).ValueGeneratedNever();

            modelBuilder.Entity<DbMetaQuestionStatistics>().ToTable("MetaQuestionStatistics");
            modelBuilder.Entity<DbMetaQuestionStatistics>().HasKey(mqs => mqs.Id);
            modelBuilder.Entity<DbMetaQuestionStatistics>().HasOne(mqs => mqs.MetaQuestion).WithMany(mq => mq.Statistics).HasForeignKey(mqs => mqs.MetaQuestionId);

            modelBuilder.Entity<DbMetaQuestionTag>().ToTable("MetaQuestionTags");
            modelBuilder.Entity<DbMetaQuestionTag>().HasKey(mqt => new { mqt.MetaQuestionId, mqt.TagName });
            modelBuilder.Entity<DbMetaQuestionTag>().HasOne(mqt => mqt.TrackingStatus).WithMany(rt => rt.MetaQuestionTags).HasForeignKey(mqt => mqt.TrackingStatusId);
            modelBuilder.Entity<DbMetaQuestionTag>().HasOne(mqt => mqt.Tag).WithMany(t => t.MetaQuestionTags).HasForeignKey(mqt => mqt.TagName);
            modelBuilder.Entity<DbMetaQuestionTag>().HasOne(mqt => mqt.MetaQuestion).WithMany(mq => mq.MetaQuestionTags).HasForeignKey(mqt => mqt.MetaQuestionId);
            modelBuilder.Entity<DbMetaQuestionTag>().HasOne(mqt => mqt.SecondaryTag).WithMany(t => t.MetaQuestionSecondaryTags).IsRequired(false).HasForeignKey(mqt => mqt.SecondaryTagName);

            modelBuilder.Entity<DbMetaQuestionTagTrackingStatus>().ToTable("MetaQuestionTagStatuses");
            modelBuilder.Entity<DbMetaQuestionTagTrackingStatus>().HasKey(mqts => mqts.Id);

            modelBuilder.Entity<DbTag>().ToTable("Tags");
            modelBuilder.Entity<DbTag>().HasKey(tag => tag.Name);
            modelBuilder.Entity<DbTag>().HasOne(tag => tag.SynonymOf).WithMany(tag => tag.Synonyms).IsRequired(false).HasForeignKey(tag => tag.SynonymOfTagName);

            modelBuilder.Entity<DbTagStatistics>().ToTable("TagStatistics");
            modelBuilder.Entity<DbTagStatistics>().HasKey(ts => ts.Id);
            modelBuilder.Entity<DbTagStatistics>().HasOne(ts => ts.Tag).WithMany(t => t.Statistics).HasForeignKey(ts => ts.TagName);

            modelBuilder.Entity<DbMetaTag>().ToTable("MetaTags");
            modelBuilder.Entity<DbMetaTag>().HasKey(tag => tag.Name);

            modelBuilder.Entity<DbBurnakiFollow>().ToTable("BurnakiFollows");
            modelBuilder.Entity<DbBurnakiFollow>().HasKey(tag => tag.Id);

            modelBuilder.Entity<DbMetaQuestionTagTrackingStatusAudit>().ToTable("MetaQuestionTagTrackingStatusAudits");
            modelBuilder.Entity<DbMetaQuestionTagTrackingStatusAudit>().HasKey(audit => audit.Id);
            modelBuilder.Entity<DbMetaQuestionTagTrackingStatusAudit>().Property(audit => audit.TimeChanged).IsRequired();
            modelBuilder.Entity<DbMetaQuestionTagTrackingStatusAudit>().HasOne(audit => audit.ChangedByUser).WithMany(u => u.TagTrackingStatusAudits).HasForeignKey(audit => audit.ChangedByUserId);
            modelBuilder.Entity<DbMetaQuestionTagTrackingStatusAudit>().HasOne(audit => audit.MetaQuestion).WithMany(mqt => mqt.TagTrackingStatusAudits).HasForeignKey(audit => audit.MetaQuestionId);
            modelBuilder.Entity<DbMetaQuestionTagTrackingStatusAudit>().HasOne(audit => audit.MetaQuestionTag).WithMany(mqt => mqt.TagTrackingStatusAudits).HasForeignKey(audit => new { audit.MetaQuestionId, audit.Tag });
            modelBuilder.Entity<DbMetaQuestionTagTrackingStatusAudit>().HasOne(audit => audit.PreviousTrackingStatus).WithMany(ts => ts.PreviousTagTrackingStatusAudits).HasForeignKey(audit => audit.PreviousTrackingStatusId).IsRequired(false);
            modelBuilder.Entity<DbMetaQuestionTagTrackingStatusAudit>().HasOne(audit => audit.NewTrackingStatus).WithMany(ts => ts.NewTagTrackingStatusAudits).HasForeignKey(audit => audit.NewTrackingStatusId);

            modelBuilder.Entity<DbUserAction>().ToTable("UserActions");
            modelBuilder.Entity<DbUserAction>().HasKey(ua => ua.Id);
            modelBuilder.Entity<DbUserAction>().HasOne(ua => ua.UserActionType).WithMany(uat => uat.UserActions).HasForeignKey(ua => ua.UserActionTypeId);
            modelBuilder.Entity<DbUserAction>().HasOne(ua => ua.SiteUser).WithMany(uat => uat.UserActions).HasForeignKey(ua => ua.SiteUserId);
            modelBuilder.Entity<DbUserAction>().HasOne(ua => ua.UnknownDeletion).WithMany(ud => ud.UserActions).HasForeignKey(ua => ua.UnknownDeletionId).IsRequired(false);
            modelBuilder.Entity<DbUserAction>().Property(ua => ua.TimeProcessed).IsRequired().HasDefaultValueSql("now() at time zone 'utc'");

            modelBuilder.Entity<DbUserActionType>().ToTable("UserActionTypes");
            modelBuilder.Entity<DbUserActionType>().HasKey(uat => uat.Id);

            modelBuilder.Entity<DbSiteUser>().ToTable("SiteUsers");
            modelBuilder.Entity<DbSiteUser>().HasKey(su => su.Id);
            modelBuilder.Entity<DbSiteUser>().Property(rt => rt.Id).ValueGeneratedNever();

            modelBuilder.Entity<DbLog>().ToTable("Logs");
            modelBuilder.Entity<DbLog>().HasKey(tag => tag.Id);
            modelBuilder.Entity<DbLog>().HasIndex(tag => tag.TimeLogged);

            modelBuilder.Entity<DbSiteUserRole>().ToTable("SiteUserRoles");
            modelBuilder.Entity<DbSiteUserRole>().HasOne(sur => sur.User).WithMany(u => u.Roles).HasForeignKey(sur => sur.UserId);
            modelBuilder.Entity<DbSiteUserRole>().HasOne(sur => sur.AddedByUser).WithMany(u => u.AddedRoles).HasForeignKey(sur => sur.AddedByUserId);
            modelBuilder.Entity<DbSiteUserRole>().HasOne(sur => sur.Role).WithMany(u => u.SiteUserRoles).HasForeignKey(sur => sur.RoleName);
            modelBuilder.Entity<DbSiteUserRole>().Property(sur => sur.AddedByUserId).HasDefaultValue(-1);
            modelBuilder.Entity<DbSiteUserRole>().Property(sur => sur.DateAdded).HasDefaultValueSql("now() at time zone 'utc'");
            modelBuilder.Entity<DbSiteUserRole>().HasKey(sur => new { sur.UserId, sur.RoleName });

            modelBuilder.Entity<DbRole>().ToTable("Roles");
            modelBuilder.Entity<DbRole>().HasKey(role => role.Name);

            modelBuilder.Entity<DbMetaQuestionMetaTag>().ToTable("MetaQuestionMetaTags");
            modelBuilder.Entity<DbMetaQuestionMetaTag>().HasKey(mqmt => new { mqmt.MetaQuestionId, mqmt.TagName });
            modelBuilder.Entity<DbMetaQuestionMetaTag>().HasOne(mqmt => mqmt.MetaQuestion).WithMany(mq => mq.MetaQuestionMetaTags);
            modelBuilder.Entity<DbMetaQuestionMetaTag>().HasOne(mqmt => mqmt.MetaTag).WithMany(mq => mq.MetaQuestionMetaTags).HasForeignKey(mqmt => mqmt.TagName);

            modelBuilder.Entity<DbUnknownDeletion>().ToTable("UnknownDeletions");
            modelBuilder.Entity<DbUnknownDeletion>().HasKey(ud => ud.Id);
            modelBuilder.Entity<DbUnknownDeletion>().HasOne(ud => ud.ProcessedByUser).WithMany(u => u.ProcessedUnknownDeletions).HasForeignKey(ua => ua.ProcessedByUserId).IsRequired(false);

            modelBuilder.Entity<DbSeenQuestion>().ToTable("SeenQuestions");
            modelBuilder.Entity<DbSeenQuestion>().HasKey(sq => new { sq.Id, sq.Tag });
            modelBuilder.Entity<DbSeenQuestion>().HasIndex(sq => sq.Tag);

            modelBuilder.Entity<DbMetaTag>()
                .HasData(
                    new DbMetaTag {Name = DbMetaTag.STATUS_COMPLETED},
                    new DbMetaTag {Name = DbMetaTag.STATUS_PLANNED},
                    new DbMetaTag {Name = DbMetaTag.STATUS_DECLINED},
                    new DbMetaTag {Name = DbMetaTag.STATUS_FEATURED}
                );

            modelBuilder.Entity<DbMetaQuestionTagTrackingStatus>()
                .HasData(
                    new DbMetaQuestionTagTrackingStatus {Id = DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL, Name = "Requires tracking approval" },
                    new DbMetaQuestionTagTrackingStatus {Id = DbMetaQuestionTagTrackingStatus.TRACKED, Name = "Tracked"},
                    new DbMetaQuestionTagTrackingStatus {Id = DbMetaQuestionTagTrackingStatus.IGNORED, Name = "Ignored"},
                    new DbMetaQuestionTagTrackingStatus { Id = DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE, Name = "Tracked elsewhere" }
                );

            modelBuilder.Entity<DbUserActionType>()
                .HasData(
                    new DbUserActionType {Id = DbUserActionType.REMOVED_TAG, Name = "Removed Tag"},
                    new DbUserActionType {Id = DbUserActionType.ADDED_TAG, Name = "Added Tag"},
                    new DbUserActionType {Id = DbUserActionType.CLOSED, Name = "Closed"},
                    new DbUserActionType {Id = DbUserActionType.REOPENED, Name = "Reopened"},
                    new DbUserActionType {Id = DbUserActionType.DELETED, Name = "Deleted"},
                    new DbUserActionType {Id = DbUserActionType.UNDELETED, Name = "Undeleted"}
                );

            modelBuilder.Entity<DbRole>()
                .HasData(
                    new DbRole {Name = DbRole.TROGDOR_ROOM_OWNER },
                    new DbRole { Name = DbRole.MODERATOR },
                    new DbRole { Name = DbRole.RODGORT_ADMIN }
                );
        }
    }
}
