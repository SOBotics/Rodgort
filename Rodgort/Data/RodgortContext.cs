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
        public DbSet<DbRequestType> RequestTypes { get; set; }
        public DbSet<DbTagStatistics> TagStatistics { get; set; }
        public DbSet<DbLog> Logs { get; set; }
        public DbSet<DbBurnakiFollow> BurnakiFollows { get; set; }
        public DbSet<DbUserAction> UserActions { get; set; }
        public DbSet<DbSiteUser> SiteUsers { get; set; }

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
            modelBuilder.Entity<DbMetaQuestionTag>().HasOne(mqt => mqt.RequestType).WithMany(rt => rt.MetaQuestionTags).HasForeignKey(mqt => mqt.RequestTypeId);
            modelBuilder.Entity<DbMetaQuestionTag>().HasOne(mqt => mqt.Status).WithMany(rt => rt.MetaQuestionTags).HasForeignKey(mqt => mqt.StatusId);
            modelBuilder.Entity<DbMetaQuestionTag>().HasOne(mqt => mqt.Tag).WithMany(t => t.MetaQuestionTags).HasForeignKey(mqt => mqt.TagName);
            modelBuilder.Entity<DbMetaQuestionTag>().HasOne(mqt => mqt.MetaQuestion).WithMany(mq => mq.MetaQuestionTags).HasForeignKey(mqt => mqt.MetaQuestionId);
            modelBuilder.Entity<DbMetaQuestionTag>().HasOne(mqt => mqt.SecondaryTag).WithMany(t => t.MetaQuestionSecondaryTags).IsRequired(false).HasForeignKey(mqt => mqt.SecondaryTagName);

            modelBuilder.Entity<DbMetaQuestionTagStatus>().ToTable("MetaQuestionTagStatuses");
            modelBuilder.Entity<DbMetaQuestionTagStatus>().HasKey(mqts => mqts.Id);

            modelBuilder.Entity<DbRequestType>().ToTable("RequestTypes");
            modelBuilder.Entity<DbRequestType>().HasKey(rt => rt.Id);
            modelBuilder.Entity<DbRequestType>().Property(rt => rt.Id).ValueGeneratedNever();

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

            modelBuilder.Entity<DbUserAction>().ToTable("UserActions");
            modelBuilder.Entity<DbUserAction>().HasKey(ua => ua.Id);
            modelBuilder.Entity<DbUserAction>().HasOne(ua => ua.UserActionType).WithMany(uat => uat.UserActions).HasForeignKey(ua => ua.UserActionTypeId);
            modelBuilder.Entity<DbUserAction>().HasOne(ua => ua.SiteUser).WithMany(uat => uat.UserActions).HasForeignKey(ua => ua.SiteUserId);

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
            modelBuilder.Entity<DbSiteUserRole>().HasOne(sur => sur.Role).WithMany(u => u.SiteUserRoles).HasForeignKey(sur => sur.RoleName);
            modelBuilder.Entity<DbSiteUserRole>().HasKey(sur => new { sur.UserId, sur.RoleName });

            modelBuilder.Entity<DbRole>().ToTable("Roles");
            modelBuilder.Entity<DbRole>().HasKey(role => role.Name);

            modelBuilder.Entity<DbMetaQuestionMetaTag>().ToTable("MetaQuestionMetaTags");
            modelBuilder.Entity<DbMetaQuestionMetaTag>().HasKey(mqmt => new { mqmt.MetaQuestionId, mqmt.TagName });
            modelBuilder.Entity<DbMetaQuestionMetaTag>().HasOne(mqmt => mqmt.MetaQuestion).WithMany(mq => mq.MetaQuestionMetaTags);
            modelBuilder.Entity<DbMetaQuestionMetaTag>().HasOne(mqmt => mqmt.MetaTag).WithMany(mq => mq.MetaQuestionMetaTags).HasForeignKey(mqmt => mqmt.TagName);

            modelBuilder
                .Entity<DbRequestType>()
                .HasData(
                    new DbRequestType {Id = DbRequestType.UNKNOWN, Name = "Unknown"},
                    new DbRequestType {Id = DbRequestType.SYNONYM, Name = "Synonym"},
                    new DbRequestType {Id = DbRequestType.MERGE, Name = "Merge"},
                    new DbRequestType {Id = DbRequestType.BURNINATE, Name = "Burninate"}
                );

            modelBuilder.Entity<DbMetaTag>()
                .HasData(
                    new DbMetaTag {Name = DbMetaTag.STATUS_COMPLETED},
                    new DbMetaTag {Name = DbMetaTag.STATUS_PLANNED},
                    new DbMetaTag {Name = DbMetaTag.STATUS_DECLINED},
                    new DbMetaTag {Name = DbMetaTag.STATUS_FEATURED}
                );

            modelBuilder.Entity<DbMetaQuestionTagStatus>()
                .HasData(
                    new DbMetaQuestionTagStatus {Id = DbMetaQuestionTagStatus.PENDING, Name = "Pending"},
                    new DbMetaQuestionTagStatus {Id = DbMetaQuestionTagStatus.APPROVED, Name = "Approved"},
                    new DbMetaQuestionTagStatus {Id = DbMetaQuestionTagStatus.REJECTED, Name = "Rejected"}
                );

            modelBuilder.Entity<DbUserActionType>()
                .HasData(
                    new DbUserActionType {Id = DbUserActionType.REMOVED_TAG, Name = "Removed Tag"},
                    new DbUserActionType {Id = DbUserActionType.ADDED_TAG, Name = "Added Tag"},
                    new DbUserActionType {Id = DbUserActionType.CLOSED, Name = "Closed"},
                    new DbUserActionType {Id = DbUserActionType.REOPENED, Name = "Reopened"},
                    new DbUserActionType {Id = DbUserActionType.DELETED, Name = "Deleted"},
                    new DbUserActionType {Id = DbUserActionType.UNDELETED, Name = "Undeleted"},
                    new DbUserActionType {Id = DbUserActionType.UNKNOWN_DELETION, Name = "Unknown deletion"}
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
