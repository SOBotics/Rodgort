using Microsoft.EntityFrameworkCore;
using Rodgort.Data.Tables;

namespace Rodgort.Data
{
    public class RodgortContext : DbContext
    {
        public RodgortContext(DbContextOptions<RodgortContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder
                .Entity<DbMetaAnswer>()
                .HasKey(ma => ma.Id);
            modelBuilder
                .Entity<DbMetaAnswer>()
                .Property(rt => rt.Id)
                .ValueGeneratedNever();
            modelBuilder
                .Entity<DbMetaAnswer>()
                .HasOne(ma => ma.MetaQuestion)
                .WithMany(mq => mq.MetaAnswers)
                .HasForeignKey(ma => ma.MetaQuestionId);
            
            modelBuilder
                .Entity<DbMetaAnswerStatistics>()
                .HasKey(mas => mas.Id);
            modelBuilder
                .Entity<DbMetaAnswerStatistics>()
                .HasOne(mas => mas.MetaAnswer)
                .WithMany(ma => ma.Statistics)
                .HasForeignKey(mas => mas.MetaAnswerId);

            modelBuilder
                .Entity<DbMetaQuestion>()
                .HasKey(mqs => mqs.Id);
            modelBuilder
                .Entity<DbMetaQuestion>()
                .Property(rt => rt.Id)
                .ValueGeneratedNever();

            modelBuilder
                .Entity<DbMetaQuestionStatistics>()
                .HasKey(mqs => mqs.Id);
            modelBuilder
                .Entity<DbMetaQuestionStatistics>()
                .HasOne(mqs => mqs.MetaQuestion)
                .WithMany(mq => mq.Statistics)
                .HasForeignKey(mqs => mqs.MetaQuestionId);

            modelBuilder
                .Entity<DbMetaQuestionTag>()
                .HasKey(mqt => new { mqt.MetaQuestionId, mqt.TagName });
            modelBuilder
                .Entity<DbMetaQuestionTag>()
                .HasOne(mqt => mqt.RequestType)
                .WithMany(rt => rt.MetaQuestionTags)
                .HasForeignKey(mqt => mqt.RequestTypeId);
            modelBuilder
                .Entity<DbMetaQuestionTag>()
                .HasOne(mqt => mqt.Tag)
                .WithMany(t => t.MetaQuestionTags)
                .HasForeignKey(mqt => mqt.TagName);
            modelBuilder
                .Entity<DbMetaQuestionTag>()
                .HasOne(mqt => mqt.MetaQuestion)
                .WithMany(mq => mq.MetaQuestionTags)
                .HasForeignKey(mqt => mqt.MetaQuestionId);
            modelBuilder
                .Entity<DbMetaQuestionTag>()
                .HasOne(mqt => mqt.SecondaryTag)
                .WithMany(t => t.MetaQuestionSecondaryTags)
                .IsRequired(false)
                .HasForeignKey(mqt => mqt.SecondaryTagName);

            modelBuilder
                .Entity<DbRequestType>()
                .HasKey(rt => rt.Id);

            modelBuilder
                .Entity<DbRequestType>()
                .Property(rt => rt.Id)
                .ValueGeneratedNever();

            modelBuilder
                .Entity<DbTag>()
                .HasKey(tag => tag.Name);

            modelBuilder
                .Entity<DbTagStatistics>()
                .HasKey(ts => ts.Id);
            modelBuilder
                .Entity<DbTagStatistics>()
                .HasOne(ts => ts.Tag)
                .WithMany(t => t.Statistics)
                .HasForeignKey(ts => ts.TagName);

            modelBuilder
                .Entity<DbRequestType>()
                .HasData(
                    new DbRequestType {Id = DbRequestType.UNKNOWN, Name = "Unknown"},
                    new DbRequestType {Id = DbRequestType.SYNONYM, Name = "Synonym"},
                    new DbRequestType {Id = DbRequestType.MERGE, Name = "Merge"},
                    new DbRequestType {Id = DbRequestType.BURNINATE, Name = "Burninate"}
                );
        }
    }
}
