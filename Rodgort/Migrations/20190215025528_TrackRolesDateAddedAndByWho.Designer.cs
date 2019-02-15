﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Rodgort.Data;

namespace Rodgort.Migrations
{
    [DbContext(typeof(RodgortContext))]
    [Migration("20190215025528_TrackRolesDateAddedAndByWho")]
    partial class TrackRolesDateAddedAndByWho
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Rodgort.Data.Tables.DbBurnakiFollow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BurnakiId");

                    b.Property<DateTime?>("FollowEnded");

                    b.Property<DateTime>("FollowStarted");

                    b.Property<int>("RoomId");

                    b.Property<string>("Tag");

                    b.HasKey("Id");

                    b.ToTable("BurnakiFollows");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Callsite");

                    b.Property<string>("Exception");

                    b.Property<string>("Level");

                    b.Property<string>("Logger");

                    b.Property<string>("Message");

                    b.Property<DateTime>("TimeLogged");

                    b.HasKey("Id");

                    b.HasIndex("TimeLogged");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaAnswer", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Body");

                    b.Property<DateTime>("LastSeen");

                    b.Property<int>("MetaQuestionId");

                    b.HasKey("Id");

                    b.HasIndex("MetaQuestionId");

                    b.ToTable("MetaAnswers");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaAnswerStatistics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateTime");

                    b.Property<int>("MetaAnswerId");

                    b.Property<int>("Score");

                    b.HasKey("Id");

                    b.HasIndex("MetaAnswerId");

                    b.ToTable("MetaAnswerStatistics");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestion", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Body");

                    b.Property<DateTime?>("BurnEnded");

                    b.Property<DateTime?>("BurnStarted");

                    b.Property<string>("CloseReason");

                    b.Property<DateTime?>("ClosedDate");

                    b.Property<DateTime?>("FeaturedEnded");

                    b.Property<DateTime?>("FeaturedStarted");

                    b.Property<DateTime>("LastSeen");

                    b.Property<string>("Link");

                    b.Property<int>("Score");

                    b.Property<string>("Title");

                    b.Property<int>("ViewCount");

                    b.HasKey("Id");

                    b.ToTable("MetaQuestions");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionMetaTag", b =>
                {
                    b.Property<int>("MetaQuestionId");

                    b.Property<string>("TagName");

                    b.HasKey("MetaQuestionId", "TagName");

                    b.HasIndex("TagName");

                    b.ToTable("MetaQuestionMetaTags");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionStatistics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateTime");

                    b.Property<int>("MetaQuestionId");

                    b.Property<int>("Score");

                    b.Property<int>("ViewCount");

                    b.HasKey("Id");

                    b.HasIndex("MetaQuestionId");

                    b.ToTable("MetaQuestionStatistics");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionTag", b =>
                {
                    b.Property<int>("MetaQuestionId");

                    b.Property<string>("TagName");

                    b.Property<string>("SecondaryTagName");

                    b.Property<int>("TrackingStatusId");

                    b.HasKey("MetaQuestionId", "TagName");

                    b.HasIndex("SecondaryTagName");

                    b.HasIndex("TagName");

                    b.HasIndex("TrackingStatusId");

                    b.ToTable("MetaQuestionTags");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("MetaQuestionTagStatuses");

                    b.HasData(
                        new { Id = 1, Name = "Requires tracking approval" },
                        new { Id = 2, Name = "Tracked" },
                        new { Id = 3, Name = "Ignored" },
                        new { Id = 4, Name = "Tracked elsewhere" }
                    );
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatusAudit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ChangedByUserId");

                    b.Property<int>("MetaQuestionId");

                    b.Property<int>("NewTrackingStatusId");

                    b.Property<int?>("PreviousTrackingStatusId");

                    b.Property<string>("Tag");

                    b.Property<DateTime>("TimeChanged");

                    b.HasKey("Id");

                    b.HasIndex("ChangedByUserId");

                    b.HasIndex("NewTrackingStatusId");

                    b.HasIndex("PreviousTrackingStatusId");

                    b.HasIndex("MetaQuestionId", "Tag");

                    b.ToTable("MetaQuestionTagTrackingStatusAudits");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaTag", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Name");

                    b.ToTable("MetaTags");

                    b.HasData(
                        new { Name = "status-completed" },
                        new { Name = "status-planned" },
                        new { Name = "status-declined" },
                        new { Name = "featured" }
                    );
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbRole", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Name");

                    b.ToTable("Roles");

                    b.HasData(
                        new { Name = "Trogdor Room Owner" },
                        new { Name = "Moderator" },
                        new { Name = "Rodgort Admin" }
                    );
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbSeenQuestion", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Tag");

                    b.Property<DateTime>("LastSeen");

                    b.HasKey("Id", "Tag");

                    b.HasIndex("Tag");

                    b.ToTable("SeenQuestions");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbSiteUser", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("DisplayName");

                    b.HasKey("Id");

                    b.ToTable("SiteUsers");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbSiteUserRole", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<string>("RoleName");

                    b.Property<int>("AddedByUserId")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(-1);

                    b.Property<DateTime>("DateAdded")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("now() at time zone 'utc'");

                    b.HasKey("UserId", "RoleName");

                    b.HasIndex("AddedByUserId");

                    b.HasIndex("RoleName");

                    b.ToTable("SiteUserRoles");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbTag", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("NumberOfQuestions");

                    b.Property<string>("SynonymOfTagName");

                    b.HasKey("Name");

                    b.HasIndex("SynonymOfTagName");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbTagStatistics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AnswerCount");

                    b.Property<DateTime>("DateTime");

                    b.Property<int>("QuestionCount");

                    b.Property<string>("TagName");

                    b.HasKey("Id");

                    b.HasIndex("TagName");

                    b.ToTable("TagStatistics");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUnknownDeletion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("PostId");

                    b.Property<DateTime?>("Processed");

                    b.Property<int?>("ProcessedByUserId");

                    b.Property<string>("Tag");

                    b.Property<DateTime>("Time");

                    b.HasKey("Id");

                    b.HasIndex("ProcessedByUserId");

                    b.ToTable("UnknownDeletions");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUserAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("PostId");

                    b.Property<int>("SiteUserId");

                    b.Property<string>("Tag");

                    b.Property<DateTime>("Time");

                    b.Property<DateTime>("TimeProcessed")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("now() at time zone 'utc'");

                    b.Property<int?>("UnknownDeletionId");

                    b.Property<int>("UserActionTypeId");

                    b.HasKey("Id");

                    b.HasIndex("SiteUserId");

                    b.HasIndex("UnknownDeletionId");

                    b.HasIndex("UserActionTypeId");

                    b.ToTable("UserActions");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUserActionType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("UserActionTypes");

                    b.HasData(
                        new { Id = 1, Name = "Removed Tag" },
                        new { Id = 2, Name = "Added Tag" },
                        new { Id = 3, Name = "Closed" },
                        new { Id = 4, Name = "Reopened" },
                        new { Id = 5, Name = "Deleted" },
                        new { Id = 6, Name = "Undeleted" }
                    );
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaAnswer", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestion", "MetaQuestion")
                        .WithMany("MetaAnswers")
                        .HasForeignKey("MetaQuestionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaAnswerStatistics", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbMetaAnswer", "MetaAnswer")
                        .WithMany("Statistics")
                        .HasForeignKey("MetaAnswerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionMetaTag", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestion", "MetaQuestion")
                        .WithMany("MetaQuestionMetaTags")
                        .HasForeignKey("MetaQuestionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbMetaTag", "MetaTag")
                        .WithMany("MetaQuestionMetaTags")
                        .HasForeignKey("TagName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionStatistics", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestion", "MetaQuestion")
                        .WithMany("Statistics")
                        .HasForeignKey("MetaQuestionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionTag", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestion", "MetaQuestion")
                        .WithMany("MetaQuestionTags")
                        .HasForeignKey("MetaQuestionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbTag", "SecondaryTag")
                        .WithMany("MetaQuestionSecondaryTags")
                        .HasForeignKey("SecondaryTagName");

                    b.HasOne("Rodgort.Data.Tables.DbTag", "Tag")
                        .WithMany("MetaQuestionTags")
                        .HasForeignKey("TagName")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatus", "TrackingStatus")
                        .WithMany("MetaQuestionTags")
                        .HasForeignKey("TrackingStatusId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatusAudit", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "ChangedByUser")
                        .WithMany("TagTrackingStatusAudits")
                        .HasForeignKey("ChangedByUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestion", "MetaQuestion")
                        .WithMany("TagTrackingStatusAudits")
                        .HasForeignKey("MetaQuestionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatus", "NewTrackingStatus")
                        .WithMany("NewTagTrackingStatusAudits")
                        .HasForeignKey("NewTrackingStatusId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatus", "PreviousTrackingStatus")
                        .WithMany("PreviousTagTrackingStatusAudits")
                        .HasForeignKey("PreviousTrackingStatusId")
                        .HasConstraintName("FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTagStatuse~1");

                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestionTag", "MetaQuestionTag")
                        .WithMany("TagTrackingStatusAudits")
                        .HasForeignKey("MetaQuestionId", "Tag");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbSiteUserRole", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "AddedByUser")
                        .WithMany("AddedRoles")
                        .HasForeignKey("AddedByUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbRole", "Role")
                        .WithMany("SiteUserRoles")
                        .HasForeignKey("RoleName")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "User")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbTag", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbTag", "SynonymOf")
                        .WithMany("Synonyms")
                        .HasForeignKey("SynonymOfTagName");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbTagStatistics", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbTag", "Tag")
                        .WithMany("Statistics")
                        .HasForeignKey("TagName");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUnknownDeletion", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "ProcessedByUser")
                        .WithMany("ProcessedUnknownDeletions")
                        .HasForeignKey("ProcessedByUserId");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUserAction", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "SiteUser")
                        .WithMany("UserActions")
                        .HasForeignKey("SiteUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbUnknownDeletion", "UnknownDeletion")
                        .WithMany("UserActions")
                        .HasForeignKey("UnknownDeletionId");

                    b.HasOne("Rodgort.Data.Tables.DbUserActionType", "UserActionType")
                        .WithMany("UserActions")
                        .HasForeignKey("UserActionTypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
