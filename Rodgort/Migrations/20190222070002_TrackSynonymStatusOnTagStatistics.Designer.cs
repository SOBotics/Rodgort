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
    [Migration("20190222070002_TrackSynonymStatusOnTagStatistics")]
    partial class TrackSynonymStatusOnTagStatistics
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
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("BurnakiId")
                        .HasColumnName("burnaki_id");

                    b.Property<DateTime?>("FollowEnded")
                        .HasColumnName("follow_ended");

                    b.Property<DateTime>("FollowStarted")
                        .HasColumnName("follow_started");

                    b.Property<int>("RoomId")
                        .HasColumnName("room_id");

                    b.Property<string>("Tag")
                        .HasColumnName("tag");

                    b.HasKey("Id")
                        .HasName("pk_burnaki_follows");

                    b.ToTable("burnaki_follows");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Callsite")
                        .HasColumnName("callsite");

                    b.Property<string>("Exception")
                        .HasColumnName("exception");

                    b.Property<string>("Level")
                        .HasColumnName("level");

                    b.Property<string>("Logger")
                        .HasColumnName("logger");

                    b.Property<string>("Message")
                        .HasColumnName("message");

                    b.Property<DateTime>("TimeLogged")
                        .HasColumnName("time_logged");

                    b.HasKey("Id")
                        .HasName("pk_logs");

                    b.HasIndex("TimeLogged");

                    b.ToTable("logs");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaAnswer", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.Property<string>("Body")
                        .HasColumnName("body");

                    b.Property<DateTime>("LastSeen")
                        .HasColumnName("last_seen");

                    b.Property<int>("MetaQuestionId")
                        .HasColumnName("meta_question_id");

                    b.HasKey("Id")
                        .HasName("pk_meta_answers");

                    b.HasIndex("MetaQuestionId")
                        .HasName("ix_meta_answers_meta_question_id");

                    b.ToTable("meta_answers");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaAnswerStatistics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime>("DateTime")
                        .HasColumnName("date_time");

                    b.Property<int>("MetaAnswerId")
                        .HasColumnName("meta_answer_id");

                    b.Property<int>("Score")
                        .HasColumnName("score");

                    b.HasKey("Id")
                        .HasName("pk_db_meta_answer_statistics");

                    b.HasIndex("MetaAnswerId")
                        .HasName("ix_db_meta_answer_statistics_meta_answer_id");

                    b.ToTable("meta_answer_statistics");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestion", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.Property<string>("Body")
                        .HasColumnName("body");

                    b.Property<DateTime?>("BurnEnded")
                        .HasColumnName("burn_ended");

                    b.Property<DateTime?>("BurnStarted")
                        .HasColumnName("burn_started");

                    b.Property<string>("CloseReason")
                        .HasColumnName("close_reason");

                    b.Property<DateTime?>("ClosedDate")
                        .HasColumnName("closed_date");

                    b.Property<DateTime?>("FeaturedEnded")
                        .HasColumnName("featured_ended");

                    b.Property<DateTime?>("FeaturedStarted")
                        .HasColumnName("featured_started");

                    b.Property<DateTime>("LastSeen")
                        .HasColumnName("last_seen");

                    b.Property<string>("Link")
                        .HasColumnName("link");

                    b.Property<int>("Score")
                        .HasColumnName("score");

                    b.Property<string>("Title")
                        .HasColumnName("title");

                    b.Property<int>("ViewCount")
                        .HasColumnName("view_count");

                    b.HasKey("Id")
                        .HasName("pk_meta_questions");

                    b.ToTable("meta_questions");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionMetaTag", b =>
                {
                    b.Property<int>("MetaQuestionId")
                        .HasColumnName("meta_question_id");

                    b.Property<string>("TagName")
                        .HasColumnName("tag_name");

                    b.HasKey("MetaQuestionId", "TagName");

                    b.HasIndex("TagName");

                    b.ToTable("meta_question_meta_tags");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionStatistics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime>("DateTime")
                        .HasColumnName("date_time");

                    b.Property<int>("MetaQuestionId")
                        .HasColumnName("meta_question_id");

                    b.Property<int>("Score")
                        .HasColumnName("score");

                    b.Property<int>("ViewCount")
                        .HasColumnName("view_count");

                    b.HasKey("Id")
                        .HasName("pk_db_meta_question_statistics");

                    b.HasIndex("MetaQuestionId")
                        .HasName("ix_db_meta_question_statistics_meta_question_id");

                    b.ToTable("meta_question_statistics");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionTag", b =>
                {
                    b.Property<int>("MetaQuestionId")
                        .HasColumnName("meta_question_id");

                    b.Property<string>("TagName")
                        .HasColumnName("tag_name");

                    b.Property<string>("SecondaryTagName")
                        .HasColumnName("secondary_tag_name");

                    b.Property<int>("TrackingStatusId")
                        .HasColumnName("tracking_status_id");

                    b.HasKey("MetaQuestionId", "TagName");

                    b.HasIndex("SecondaryTagName");

                    b.HasIndex("TagName");

                    b.HasIndex("TrackingStatusId")
                        .HasName("ix_meta_question_tags_tracking_status_id");

                    b.ToTable("meta_question_tags");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_db_meta_question_tag_tracking_status");

                    b.ToTable("metag_question_tag_statuses");

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
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("ChangedByUserId")
                        .HasColumnName("changed_by_user_id");

                    b.Property<int>("MetaQuestionId")
                        .HasColumnName("meta_question_id");

                    b.Property<int>("NewTrackingStatusId")
                        .HasColumnName("new_tracking_status_id");

                    b.Property<int?>("PreviousTrackingStatusId")
                        .HasColumnName("previous_tracking_status_id");

                    b.Property<string>("Tag")
                        .HasColumnName("tag");

                    b.Property<DateTime>("TimeChanged")
                        .HasColumnName("time_changed");

                    b.HasKey("Id")
                        .HasName("pk_meta_question_tag_tracking_status_audits");

                    b.HasIndex("ChangedByUserId")
                        .HasName("ix_meta_question_tag_tracking_status_audits_changed_by_user_id");

                    b.HasIndex("NewTrackingStatusId");

                    b.HasIndex("PreviousTrackingStatusId");

                    b.HasIndex("MetaQuestionId", "Tag");

                    b.ToTable("meta_question_tag_tracking_status_audits");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaTag", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("name");

                    b.HasKey("Name");

                    b.ToTable("meta_tags");

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
                        .ValueGeneratedOnAdd()
                        .HasColumnName("name");

                    b.HasKey("Name");

                    b.ToTable("roles");

                    b.HasData(
                        new { Name = "Trogdor Room Owner" },
                        new { Name = "Moderator" },
                        new { Name = "Rodgort Admin" }
                    );
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbSeenQuestion", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.Property<string>("Tag")
                        .HasColumnName("tag");

                    b.Property<DateTime>("LastSeen")
                        .HasColumnName("last_seen");

                    b.HasKey("Id", "Tag");

                    b.HasIndex("Tag");

                    b.ToTable("seen_questions");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbSiteUser", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id");

                    b.Property<string>("DisplayName")
                        .HasColumnName("display_name");

                    b.Property<bool>("IsModerator")
                        .HasColumnName("is_moderator");

                    b.HasKey("Id")
                        .HasName("pk_site_users");

                    b.ToTable("site_users");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbSiteUserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnName("user_id");

                    b.Property<string>("RoleName")
                        .HasColumnName("role_name");

                    b.Property<int>("AddedByUserId")
                        .HasColumnName("added_by_user_id");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnName("date_added");

                    b.Property<bool>("Enabled")
                        .HasColumnName("enabled");

                    b.HasKey("UserId", "RoleName");

                    b.HasIndex("AddedByUserId");

                    b.HasIndex("RoleName");

                    b.ToTable("site_user_roles");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbSiteUserRoleAudit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<bool>("Added")
                        .HasColumnName("added");

                    b.Property<int>("ChangedByUserId")
                        .HasColumnName("changed_by_user_id");

                    b.Property<DateTime>("DateChanged")
                        .HasColumnName("date_changed");

                    b.Property<string>("RoleName")
                        .HasColumnName("role_name");

                    b.Property<int>("UserId")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_site_user_role_audits");

                    b.HasIndex("ChangedByUserId");

                    b.HasIndex("UserId", "RoleName");

                    b.ToTable("site_user_role_audits");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbTag", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("name");

                    b.Property<int?>("NumberOfQuestions")
                        .HasColumnName("number_of_questions");

                    b.Property<string>("SynonymOfTagName")
                        .HasColumnName("synonym_of_tag_name");

                    b.HasKey("Name");

                    b.HasIndex("SynonymOfTagName");

                    b.ToTable("tags");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbTagStatistics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("AnswerCount")
                        .HasColumnName("answer_count");

                    b.Property<DateTime>("DateTime")
                        .HasColumnName("date_time");

                    b.Property<bool>("IsSynonym")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("is_synonym")
                        .HasDefaultValue(false);

                    b.Property<int>("QuestionCount")
                        .HasColumnName("question_count");

                    b.Property<string>("TagName")
                        .HasColumnName("tag_name");

                    b.HasKey("Id")
                        .HasName("pk_tag_statistics");

                    b.HasIndex("TagName");

                    b.ToTable("tag_statistics");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUnknownDeletion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("PostId")
                        .HasColumnName("post_id");

                    b.Property<DateTime?>("Processed")
                        .HasColumnName("processed");

                    b.Property<int?>("ProcessedByUserId")
                        .HasColumnName("processed_by_user_id");

                    b.Property<string>("Tag")
                        .HasColumnName("tag");

                    b.Property<DateTime>("Time")
                        .HasColumnName("time");

                    b.HasKey("Id")
                        .HasName("pk_unknown_deletions");

                    b.HasIndex("ProcessedByUserId")
                        .HasName("ix_unknown_deletions_processed_by_user_id");

                    b.ToTable("unknown_deletions");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUserAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("PostId")
                        .HasColumnName("post_id");

                    b.Property<int>("SiteUserId")
                        .HasColumnName("site_user_id");

                    b.Property<string>("Tag")
                        .HasColumnName("tag");

                    b.Property<DateTime>("Time")
                        .HasColumnName("time");

                    b.Property<DateTime>("TimeProcessed")
                        .HasColumnName("time_processed");

                    b.Property<int?>("UnknownDeletionId")
                        .HasColumnName("unknown_deletion_id");

                    b.Property<int>("UserActionTypeId")
                        .HasColumnName("user_action_type_id");

                    b.HasKey("Id")
                        .HasName("pk_user_actions");

                    b.HasIndex("SiteUserId")
                        .HasName("ix_user_actions_site_user_id");

                    b.HasIndex("UnknownDeletionId")
                        .HasName("ix_user_actions_unknown_deletion_id");

                    b.HasIndex("UserActionTypeId")
                        .HasName("ix_user_actions_user_action_type_id");

                    b.ToTable("user_actions");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUserActionType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_db_user_action_type");

                    b.ToTable("user_action_types");

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
                        .HasConstraintName("fk_meta_answers_meta_questions_meta_question_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaAnswerStatistics", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbMetaAnswer", "MetaAnswer")
                        .WithMany("Statistics")
                        .HasForeignKey("MetaAnswerId")
                        .HasConstraintName("fk_db_meta_answer_statistics_meta_answers_meta_answer_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionMetaTag", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestion", "MetaQuestion")
                        .WithMany("MetaQuestionMetaTags")
                        .HasForeignKey("MetaQuestionId")
                        .HasConstraintName("fk_meta_question_meta_tags_meta_questions_meta_question_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbMetaTag", "MetaTag")
                        .WithMany("MetaQuestionMetaTags")
                        .HasForeignKey("TagName")
                        .HasConstraintName("fk_meta_question_meta_tags_meta_tags_meta_tag_temp_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionStatistics", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestion", "MetaQuestion")
                        .WithMany("Statistics")
                        .HasForeignKey("MetaQuestionId")
                        .HasConstraintName("fk_db_meta_question_statistics_meta_questions_meta_question_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionTag", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestion", "MetaQuestion")
                        .WithMany("MetaQuestionTags")
                        .HasForeignKey("MetaQuestionId")
                        .HasConstraintName("fk_meta_question_tags_meta_questions_meta_question_id")
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
                        .HasConstraintName("fk_meta_question_tags_db_meta_question_tag_tracking_status_tracking_s~")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatusAudit", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "ChangedByUser")
                        .WithMany("TagTrackingStatusAudits")
                        .HasForeignKey("ChangedByUserId")
                        .HasConstraintName("fk_meta_question_tag_tracking_status_audits_site_users_changed_by_us~")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestion", "MetaQuestion")
                        .WithMany("TagTrackingStatusAudits")
                        .HasForeignKey("MetaQuestionId")
                        .HasConstraintName("fk_meta_question_tag_tracking_status_audits_meta_questions_meta_ques~")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatus", "NewTrackingStatus")
                        .WithMany("NewTagTrackingStatusAudits")
                        .HasForeignKey("NewTrackingStatusId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestionTagTrackingStatus", "PreviousTrackingStatus")
                        .WithMany("PreviousTagTrackingStatusAudits")
                        .HasForeignKey("PreviousTrackingStatusId")
                        .HasConstraintName("FK_meta_question_tag_tracking_status_audits_metag_question_ta~1");

                    b.HasOne("Rodgort.Data.Tables.DbMetaQuestionTag", "MetaQuestionTag")
                        .WithMany("TagTrackingStatusAudits")
                        .HasForeignKey("MetaQuestionId", "Tag")
                        .HasConstraintName("fk_meta_question_tag_tracking_status_audits_meta_question_tags_meta_q~");
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
                        .HasConstraintName("fk_site_user_roles_roles_role_temp_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "User")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbSiteUserRoleAudit", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "ChangedByUser")
                        .WithMany("ChangedOtherRoles")
                        .HasForeignKey("ChangedByUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "User")
                        .WithMany("UserRolesChanged")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbSiteUserRole", "Role")
                        .WithMany("Audits")
                        .HasForeignKey("UserId", "RoleName")
                        .HasConstraintName("fk_site_user_role_audits_site_user_roles_role_temp_id");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbTag", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbTag", "SynonymOf")
                        .WithMany("Synonyms")
                        .HasForeignKey("SynonymOfTagName")
                        .HasConstraintName("fk_tags_tags_synonym_of_temp_id1");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbTagStatistics", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbTag", "Tag")
                        .WithMany("Statistics")
                        .HasForeignKey("TagName")
                        .HasConstraintName("fk_tag_statistics_tags_tag_temp_id");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUnknownDeletion", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "ProcessedByUser")
                        .WithMany("ProcessedUnknownDeletions")
                        .HasForeignKey("ProcessedByUserId")
                        .HasConstraintName("fk_unknown_deletions_site_users_processed_by_user_id");
                });

            modelBuilder.Entity("Rodgort.Data.Tables.DbUserAction", b =>
                {
                    b.HasOne("Rodgort.Data.Tables.DbSiteUser", "SiteUser")
                        .WithMany("UserActions")
                        .HasForeignKey("SiteUserId")
                        .HasConstraintName("fk_user_actions_site_users_site_user_id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Rodgort.Data.Tables.DbUnknownDeletion", "UnknownDeletion")
                        .WithMany("UserActions")
                        .HasForeignKey("UnknownDeletionId")
                        .HasConstraintName("fk_user_actions_unknown_deletions_unknown_deletion_id");

                    b.HasOne("Rodgort.Data.Tables.DbUserActionType", "UserActionType")
                        .WithMany("UserActions")
                        .HasForeignKey("UserActionTypeId")
                        .HasConstraintName("fk_user_actions_db_user_action_type_user_action_type_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
