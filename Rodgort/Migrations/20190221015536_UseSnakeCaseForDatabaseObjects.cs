using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class UseSnakeCaseForDatabaseObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetaAnswers_MetaQuestions_MetaQuestionId",
                table: "MetaAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaAnswerStatistics_MetaAnswers_MetaAnswerId",
                table: "MetaAnswerStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionMetaTags_MetaQuestions_MetaQuestionId",
                table: "MetaQuestionMetaTags");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionMetaTags_MetaTags_TagName",
                table: "MetaQuestionMetaTags");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionStatistics_MetaQuestions_MetaQuestionId",
                table: "MetaQuestionStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestions_MetaQuestionId",
                table: "MetaQuestionTags");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTags_Tags_SecondaryTagName",
                table: "MetaQuestionTags");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTags_Tags_TagName",
                table: "MetaQuestionTags");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestionTagStatuses_TrackingStatusId",
                table: "MetaQuestionTags");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_SiteUsers_ChangedByUser~",
                table: "MetaQuestionTagTrackingStatusAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestions_MetaQuest~",
                table: "MetaQuestionTagTrackingStatusAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTagStatuses~",
                table: "MetaQuestionTagTrackingStatusAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTagStatuse~1",
                table: "MetaQuestionTagTrackingStatusAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTags_MetaQu~",
                table: "MetaQuestionTagTrackingStatusAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteUserRoleAudits_SiteUsers_ChangedByUserId",
                table: "SiteUserRoleAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteUserRoleAudits_SiteUsers_UserId",
                table: "SiteUserRoleAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteUserRoleAudits_SiteUserRoles_UserId_RoleName",
                table: "SiteUserRoleAudits");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteUserRoles_SiteUsers_AddedByUserId",
                table: "SiteUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteUserRoles_Roles_RoleName",
                table: "SiteUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteUserRoles_SiteUsers_UserId",
                table: "SiteUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tags_SynonymOfTagName",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_TagStatistics_Tags_TagName",
                table: "TagStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_UnknownDeletions_SiteUsers_ProcessedByUserId",
                table: "UnknownDeletions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActions_SiteUsers_SiteUserId",
                table: "UserActions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActions_UnknownDeletions_UnknownDeletionId",
                table: "UserActions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActions_UserActionTypes_UserActionTypeId",
                table: "UserActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Logs",
                table: "Logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActionTypes",
                table: "UserActionTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActions",
                table: "UserActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UnknownDeletions",
                table: "UnknownDeletions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TagStatistics",
                table: "TagStatistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteUsers",
                table: "SiteUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteUserRoles",
                table: "SiteUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteUserRoleAudits",
                table: "SiteUserRoleAudits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SeenQuestions",
                table: "SeenQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaTags",
                table: "MetaTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaQuestionTagTrackingStatusAudits",
                table: "MetaQuestionTagTrackingStatusAudits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaQuestionTagStatuses",
                table: "MetaQuestionTagStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaQuestionTags",
                table: "MetaQuestionTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaQuestionStatistics",
                table: "MetaQuestionStatistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaQuestions",
                table: "MetaQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaQuestionMetaTags",
                table: "MetaQuestionMetaTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaAnswerStatistics",
                table: "MetaAnswerStatistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaAnswers",
                table: "MetaAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BurnakiFollows",
                table: "BurnakiFollows");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "tags");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "Logs",
                newName: "logs");

            migrationBuilder.RenameTable(
                name: "UserActionTypes",
                newName: "user_action_types");

            migrationBuilder.RenameTable(
                name: "UserActions",
                newName: "user_actions");

            migrationBuilder.RenameTable(
                name: "UnknownDeletions",
                newName: "unknown_deletions");

            migrationBuilder.RenameTable(
                name: "TagStatistics",
                newName: "tag_statistics");

            migrationBuilder.RenameTable(
                name: "SiteUsers",
                newName: "site_users");

            migrationBuilder.RenameTable(
                name: "SiteUserRoles",
                newName: "site_user_roles");

            migrationBuilder.RenameTable(
                name: "SiteUserRoleAudits",
                newName: "site_user_role_audits");

            migrationBuilder.RenameTable(
                name: "SeenQuestions",
                newName: "seen_questions");

            migrationBuilder.RenameTable(
                name: "MetaTags",
                newName: "meta_tags");

            migrationBuilder.RenameTable(
                name: "MetaQuestionTagTrackingStatusAudits",
                newName: "meta_question_tag_tracking_status_audits");

            migrationBuilder.RenameTable(
                name: "MetaQuestionTagStatuses",
                newName: "metag_question_tag_statuses");

            migrationBuilder.RenameTable(
                name: "MetaQuestionTags",
                newName: "meta_question_tags");

            migrationBuilder.RenameTable(
                name: "MetaQuestionStatistics",
                newName: "meta_question_statistics");

            migrationBuilder.RenameTable(
                name: "MetaQuestions",
                newName: "meta_questions");

            migrationBuilder.RenameTable(
                name: "MetaQuestionMetaTags",
                newName: "meta_question_meta_tags");

            migrationBuilder.RenameTable(
                name: "MetaAnswerStatistics",
                newName: "meta_answer_statistics");

            migrationBuilder.RenameTable(
                name: "MetaAnswers",
                newName: "meta_answers");

            migrationBuilder.RenameTable(
                name: "BurnakiFollows",
                newName: "burnaki_follows");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "tags",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "SynonymOfTagName",
                table: "tags",
                newName: "synonym_of_tag_name");

            migrationBuilder.RenameColumn(
                name: "NumberOfQuestions",
                table: "tags",
                newName: "number_of_questions");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_SynonymOfTagName",
                table: "tags",
                newName: "IX_tags_synonym_of_tag_name");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "roles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "logs",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Logger",
                table: "logs",
                newName: "logger");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "logs",
                newName: "level");

            migrationBuilder.RenameColumn(
                name: "Exception",
                table: "logs",
                newName: "exception");

            migrationBuilder.RenameColumn(
                name: "Callsite",
                table: "logs",
                newName: "callsite");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "logs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TimeLogged",
                table: "logs",
                newName: "time_logged");

            migrationBuilder.RenameIndex(
                name: "IX_Logs_TimeLogged",
                table: "logs",
                newName: "IX_logs_time_logged");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "user_action_types",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user_action_types",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "user_actions",
                newName: "time");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "user_actions",
                newName: "tag");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user_actions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserActionTypeId",
                table: "user_actions",
                newName: "user_action_type_id");

            migrationBuilder.RenameColumn(
                name: "UnknownDeletionId",
                table: "user_actions",
                newName: "unknown_deletion_id");

            migrationBuilder.RenameColumn(
                name: "TimeProcessed",
                table: "user_actions",
                newName: "time_processed");

            migrationBuilder.RenameColumn(
                name: "SiteUserId",
                table: "user_actions",
                newName: "site_user_id");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "user_actions",
                newName: "post_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserActions_UserActionTypeId",
                table: "user_actions",
                newName: "ix_user_actions_user_action_type_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserActions_UnknownDeletionId",
                table: "user_actions",
                newName: "ix_user_actions_unknown_deletion_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserActions_SiteUserId",
                table: "user_actions",
                newName: "ix_user_actions_site_user_id");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "unknown_deletions",
                newName: "time");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "unknown_deletions",
                newName: "tag");

            migrationBuilder.RenameColumn(
                name: "Processed",
                table: "unknown_deletions",
                newName: "processed");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "unknown_deletions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProcessedByUserId",
                table: "unknown_deletions",
                newName: "processed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "unknown_deletions",
                newName: "post_id");

            migrationBuilder.RenameIndex(
                name: "IX_UnknownDeletions_ProcessedByUserId",
                table: "unknown_deletions",
                newName: "ix_unknown_deletions_processed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tag_statistics",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TagName",
                table: "tag_statistics",
                newName: "tag_name");

            migrationBuilder.RenameColumn(
                name: "QuestionCount",
                table: "tag_statistics",
                newName: "question_count");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "tag_statistics",
                newName: "date_time");

            migrationBuilder.RenameColumn(
                name: "AnswerCount",
                table: "tag_statistics",
                newName: "answer_count");

            migrationBuilder.RenameIndex(
                name: "IX_TagStatistics_TagName",
                table: "tag_statistics",
                newName: "IX_tag_statistics_tag_name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "site_users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IsModerator",
                table: "site_users",
                newName: "is_moderator");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "site_users",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "Enabled",
                table: "site_user_roles",
                newName: "enabled");

            migrationBuilder.RenameColumn(
                name: "DateAdded",
                table: "site_user_roles",
                newName: "date_added");

            migrationBuilder.RenameColumn(
                name: "AddedByUserId",
                table: "site_user_roles",
                newName: "added_by_user_id");

            migrationBuilder.RenameColumn(
                name: "RoleName",
                table: "site_user_roles",
                newName: "role_name");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "site_user_roles",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_SiteUserRoles_RoleName",
                table: "site_user_roles",
                newName: "IX_site_user_roles_role_name");

            migrationBuilder.RenameIndex(
                name: "IX_SiteUserRoles_AddedByUserId",
                table: "site_user_roles",
                newName: "IX_site_user_roles_added_by_user_id");

            migrationBuilder.RenameColumn(
                name: "Added",
                table: "site_user_role_audits",
                newName: "added");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "site_user_role_audits",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "site_user_role_audits",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RoleName",
                table: "site_user_role_audits",
                newName: "role_name");

            migrationBuilder.RenameColumn(
                name: "DateChanged",
                table: "site_user_role_audits",
                newName: "date_changed");

            migrationBuilder.RenameColumn(
                name: "ChangedByUserId",
                table: "site_user_role_audits",
                newName: "changed_by_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_SiteUserRoleAudits_UserId_RoleName",
                table: "site_user_role_audits",
                newName: "IX_site_user_role_audits_user_id_role_name");

            migrationBuilder.RenameIndex(
                name: "IX_SiteUserRoleAudits_ChangedByUserId",
                table: "site_user_role_audits",
                newName: "IX_site_user_role_audits_changed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "seen_questions",
                newName: "tag");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "seen_questions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LastSeen",
                table: "seen_questions",
                newName: "last_seen");

            migrationBuilder.RenameIndex(
                name: "IX_SeenQuestions_Tag",
                table: "seen_questions",
                newName: "IX_seen_questions_tag");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "meta_tags",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "meta_question_tag_tracking_status_audits",
                newName: "tag");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "meta_question_tag_tracking_status_audits",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TimeChanged",
                table: "meta_question_tag_tracking_status_audits",
                newName: "time_changed");

            migrationBuilder.RenameColumn(
                name: "PreviousTrackingStatusId",
                table: "meta_question_tag_tracking_status_audits",
                newName: "previous_tracking_status_id");

            migrationBuilder.RenameColumn(
                name: "NewTrackingStatusId",
                table: "meta_question_tag_tracking_status_audits",
                newName: "new_tracking_status_id");

            migrationBuilder.RenameColumn(
                name: "MetaQuestionId",
                table: "meta_question_tag_tracking_status_audits",
                newName: "meta_question_id");

            migrationBuilder.RenameColumn(
                name: "ChangedByUserId",
                table: "meta_question_tag_tracking_status_audits",
                newName: "changed_by_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionTagTrackingStatusAudits_MetaQuestionId_Tag",
                table: "meta_question_tag_tracking_status_audits",
                newName: "IX_meta_question_tag_tracking_status_audits_meta_question_id_t~");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionTagTrackingStatusAudits_PreviousTrackingStatusId",
                table: "meta_question_tag_tracking_status_audits",
                newName: "IX_meta_question_tag_tracking_status_audits_previous_tracking_~");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionTagTrackingStatusAudits_NewTrackingStatusId",
                table: "meta_question_tag_tracking_status_audits",
                newName: "IX_meta_question_tag_tracking_status_audits_new_tracking_statu~");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionTagTrackingStatusAudits_ChangedByUserId",
                table: "meta_question_tag_tracking_status_audits",
                newName: "ix_meta_question_tag_tracking_status_audits_changed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "metag_question_tag_statuses",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "metag_question_tag_statuses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TrackingStatusId",
                table: "meta_question_tags",
                newName: "tracking_status_id");

            migrationBuilder.RenameColumn(
                name: "SecondaryTagName",
                table: "meta_question_tags",
                newName: "secondary_tag_name");

            migrationBuilder.RenameColumn(
                name: "TagName",
                table: "meta_question_tags",
                newName: "tag_name");

            migrationBuilder.RenameColumn(
                name: "MetaQuestionId",
                table: "meta_question_tags",
                newName: "meta_question_id");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionTags_TrackingStatusId",
                table: "meta_question_tags",
                newName: "ix_meta_question_tags_tracking_status_id");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionTags_TagName",
                table: "meta_question_tags",
                newName: "IX_meta_question_tags_tag_name");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionTags_SecondaryTagName",
                table: "meta_question_tags",
                newName: "IX_meta_question_tags_secondary_tag_name");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "meta_question_statistics",
                newName: "score");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "meta_question_statistics",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ViewCount",
                table: "meta_question_statistics",
                newName: "view_count");

            migrationBuilder.RenameColumn(
                name: "MetaQuestionId",
                table: "meta_question_statistics",
                newName: "meta_question_id");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "meta_question_statistics",
                newName: "date_time");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionStatistics_MetaQuestionId",
                table: "meta_question_statistics",
                newName: "ix_db_meta_question_statistics_meta_question_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "meta_questions",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "meta_questions",
                newName: "score");

            migrationBuilder.RenameColumn(
                name: "Link",
                table: "meta_questions",
                newName: "link");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "meta_questions",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "meta_questions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ViewCount",
                table: "meta_questions",
                newName: "view_count");

            migrationBuilder.RenameColumn(
                name: "LastSeen",
                table: "meta_questions",
                newName: "last_seen");

            migrationBuilder.RenameColumn(
                name: "FeaturedStarted",
                table: "meta_questions",
                newName: "featured_started");

            migrationBuilder.RenameColumn(
                name: "FeaturedEnded",
                table: "meta_questions",
                newName: "featured_ended");

            migrationBuilder.RenameColumn(
                name: "ClosedDate",
                table: "meta_questions",
                newName: "closed_date");

            migrationBuilder.RenameColumn(
                name: "CloseReason",
                table: "meta_questions",
                newName: "close_reason");

            migrationBuilder.RenameColumn(
                name: "BurnStarted",
                table: "meta_questions",
                newName: "burn_started");

            migrationBuilder.RenameColumn(
                name: "BurnEnded",
                table: "meta_questions",
                newName: "burn_ended");

            migrationBuilder.RenameColumn(
                name: "TagName",
                table: "meta_question_meta_tags",
                newName: "tag_name");

            migrationBuilder.RenameColumn(
                name: "MetaQuestionId",
                table: "meta_question_meta_tags",
                newName: "meta_question_id");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionMetaTags_TagName",
                table: "meta_question_meta_tags",
                newName: "IX_meta_question_meta_tags_tag_name");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "meta_answer_statistics",
                newName: "score");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "meta_answer_statistics",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MetaAnswerId",
                table: "meta_answer_statistics",
                newName: "meta_answer_id");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "meta_answer_statistics",
                newName: "date_time");

            migrationBuilder.RenameIndex(
                name: "IX_MetaAnswerStatistics_MetaAnswerId",
                table: "meta_answer_statistics",
                newName: "ix_db_meta_answer_statistics_meta_answer_id");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "meta_answers",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "meta_answers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MetaQuestionId",
                table: "meta_answers",
                newName: "meta_question_id");

            migrationBuilder.RenameColumn(
                name: "LastSeen",
                table: "meta_answers",
                newName: "last_seen");

            migrationBuilder.RenameIndex(
                name: "IX_MetaAnswers_MetaQuestionId",
                table: "meta_answers",
                newName: "ix_meta_answers_meta_question_id");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "burnaki_follows",
                newName: "tag");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "burnaki_follows",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "burnaki_follows",
                newName: "room_id");

            migrationBuilder.RenameColumn(
                name: "FollowStarted",
                table: "burnaki_follows",
                newName: "follow_started");

            migrationBuilder.RenameColumn(
                name: "FollowEnded",
                table: "burnaki_follows",
                newName: "follow_ended");

            migrationBuilder.RenameColumn(
                name: "BurnakiId",
                table: "burnaki_follows",
                newName: "burnaki_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tags",
                table: "tags",
                column: "name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_roles",
                table: "roles",
                column: "name");

            migrationBuilder.AddPrimaryKey(
                name: "pk_logs",
                table: "logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_db_user_action_type",
                table: "user_action_types",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_actions",
                table: "user_actions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_unknown_deletions",
                table: "unknown_deletions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_tag_statistics",
                table: "tag_statistics",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_site_users",
                table: "site_users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_site_user_roles",
                table: "site_user_roles",
                columns: new[] { "user_id", "role_name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_site_user_role_audits",
                table: "site_user_role_audits",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_seen_questions",
                table: "seen_questions",
                columns: new[] { "id", "tag" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_meta_tags",
                table: "meta_tags",
                column: "name");

            migrationBuilder.AddPrimaryKey(
                name: "pk_meta_question_tag_tracking_status_audits",
                table: "meta_question_tag_tracking_status_audits",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_db_meta_question_tag_tracking_status",
                table: "metag_question_tag_statuses",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_meta_question_tags",
                table: "meta_question_tags",
                columns: new[] { "meta_question_id", "tag_name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_db_meta_question_statistics",
                table: "meta_question_statistics",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_meta_questions",
                table: "meta_questions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_meta_question_meta_tags",
                table: "meta_question_meta_tags",
                columns: new[] { "meta_question_id", "tag_name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_db_meta_answer_statistics",
                table: "meta_answer_statistics",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_meta_answers",
                table: "meta_answers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_burnaki_follows",
                table: "burnaki_follows",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_db_meta_answer_statistics_meta_answers_meta_answer_id",
                table: "meta_answer_statistics",
                column: "meta_answer_id",
                principalTable: "meta_answers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_meta_answers_meta_questions_meta_question_id",
                table: "meta_answers",
                column: "meta_question_id",
                principalTable: "meta_questions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_meta_question_meta_tags_meta_questions_meta_question_id",
                table: "meta_question_meta_tags",
                column: "meta_question_id",
                principalTable: "meta_questions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_meta_question_meta_tags_meta_tags_meta_tag_temp_id",
                table: "meta_question_meta_tags",
                column: "tag_name",
                principalTable: "meta_tags",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_db_meta_question_statistics_meta_questions_meta_question_id",
                table: "meta_question_statistics",
                column: "meta_question_id",
                principalTable: "meta_questions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_site_users_changed_by_us~",
                table: "meta_question_tag_tracking_status_audits",
                column: "changed_by_user_id",
                principalTable: "site_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_meta_questions_meta_ques~",
                table: "meta_question_tag_tracking_status_audits",
                column: "meta_question_id",
                principalTable: "meta_questions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_meta_question_tag_tracking_status_audits_metag_question_tag~",
                table: "meta_question_tag_tracking_status_audits",
                column: "new_tracking_status_id",
                principalTable: "metag_question_tag_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_meta_question_tag_tracking_status_audits_metag_question_ta~1",
                table: "meta_question_tag_tracking_status_audits",
                column: "previous_tracking_status_id",
                principalTable: "metag_question_tag_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_meta_question_tags_meta_q~",
                table: "meta_question_tag_tracking_status_audits",
                columns: new[] { "meta_question_id", "tag" },
                principalTable: "meta_question_tags",
                principalColumns: new[] { "meta_question_id", "tag_name" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_meta_question_tags_meta_questions_meta_question_id",
                table: "meta_question_tags",
                column: "meta_question_id",
                principalTable: "meta_questions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_meta_question_tags_tags_secondary_tag_name",
                table: "meta_question_tags",
                column: "secondary_tag_name",
                principalTable: "tags",
                principalColumn: "name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_meta_question_tags_tags_tag_name",
                table: "meta_question_tags",
                column: "tag_name",
                principalTable: "tags",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_meta_question_tags_db_meta_question_tag_tracking_status_tracking_s~",
                table: "meta_question_tags",
                column: "tracking_status_id",
                principalTable: "metag_question_tag_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_site_user_role_audits_site_users_changed_by_user_id",
                table: "site_user_role_audits",
                column: "changed_by_user_id",
                principalTable: "site_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_site_user_role_audits_site_users_user_id",
                table: "site_user_role_audits",
                column: "user_id",
                principalTable: "site_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_site_user_role_audits_site_user_roles_role_temp_id",
                table: "site_user_role_audits",
                columns: new[] { "user_id", "role_name" },
                principalTable: "site_user_roles",
                principalColumns: new[] { "user_id", "role_name" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_site_user_roles_site_users_added_by_user_id",
                table: "site_user_roles",
                column: "added_by_user_id",
                principalTable: "site_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_site_user_roles_roles_role_temp_id",
                table: "site_user_roles",
                column: "role_name",
                principalTable: "roles",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_site_user_roles_site_users_user_id",
                table: "site_user_roles",
                column: "user_id",
                principalTable: "site_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tag_statistics_tags_tag_temp_id",
                table: "tag_statistics",
                column: "tag_name",
                principalTable: "tags",
                principalColumn: "name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_tags_tags_synonym_of_temp_id1",
                table: "tags",
                column: "synonym_of_tag_name",
                principalTable: "tags",
                principalColumn: "name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_unknown_deletions_site_users_processed_by_user_id",
                table: "unknown_deletions",
                column: "processed_by_user_id",
                principalTable: "site_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_user_actions_site_users_site_user_id",
                table: "user_actions",
                column: "site_user_id",
                principalTable: "site_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_actions_unknown_deletions_unknown_deletion_id",
                table: "user_actions",
                column: "unknown_deletion_id",
                principalTable: "unknown_deletions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_user_actions_db_user_action_type_user_action_type_id",
                table: "user_actions",
                column: "user_action_type_id",
                principalTable: "user_action_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_db_meta_answer_statistics_meta_answers_meta_answer_id",
                table: "meta_answer_statistics");

            migrationBuilder.DropForeignKey(
                name: "fk_meta_answers_meta_questions_meta_question_id",
                table: "meta_answers");

            migrationBuilder.DropForeignKey(
                name: "fk_meta_question_meta_tags_meta_questions_meta_question_id",
                table: "meta_question_meta_tags");

            migrationBuilder.DropForeignKey(
                name: "fk_meta_question_meta_tags_meta_tags_meta_tag_temp_id",
                table: "meta_question_meta_tags");

            migrationBuilder.DropForeignKey(
                name: "fk_db_meta_question_statistics_meta_questions_meta_question_id",
                table: "meta_question_statistics");

            migrationBuilder.DropForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_site_users_changed_by_us~",
                table: "meta_question_tag_tracking_status_audits");

            migrationBuilder.DropForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_meta_questions_meta_ques~",
                table: "meta_question_tag_tracking_status_audits");

            migrationBuilder.DropForeignKey(
                name: "FK_meta_question_tag_tracking_status_audits_metag_question_tag~",
                table: "meta_question_tag_tracking_status_audits");

            migrationBuilder.DropForeignKey(
                name: "FK_meta_question_tag_tracking_status_audits_metag_question_ta~1",
                table: "meta_question_tag_tracking_status_audits");

            migrationBuilder.DropForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_meta_question_tags_meta_q~",
                table: "meta_question_tag_tracking_status_audits");

            migrationBuilder.DropForeignKey(
                name: "fk_meta_question_tags_meta_questions_meta_question_id",
                table: "meta_question_tags");

            migrationBuilder.DropForeignKey(
                name: "FK_meta_question_tags_tags_secondary_tag_name",
                table: "meta_question_tags");

            migrationBuilder.DropForeignKey(
                name: "FK_meta_question_tags_tags_tag_name",
                table: "meta_question_tags");

            migrationBuilder.DropForeignKey(
                name: "fk_meta_question_tags_db_meta_question_tag_tracking_status_tracking_s~",
                table: "meta_question_tags");

            migrationBuilder.DropForeignKey(
                name: "FK_site_user_role_audits_site_users_changed_by_user_id",
                table: "site_user_role_audits");

            migrationBuilder.DropForeignKey(
                name: "FK_site_user_role_audits_site_users_user_id",
                table: "site_user_role_audits");

            migrationBuilder.DropForeignKey(
                name: "fk_site_user_role_audits_site_user_roles_role_temp_id",
                table: "site_user_role_audits");

            migrationBuilder.DropForeignKey(
                name: "FK_site_user_roles_site_users_added_by_user_id",
                table: "site_user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_site_user_roles_roles_role_temp_id",
                table: "site_user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_site_user_roles_site_users_user_id",
                table: "site_user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_tag_statistics_tags_tag_temp_id",
                table: "tag_statistics");

            migrationBuilder.DropForeignKey(
                name: "fk_tags_tags_synonym_of_temp_id1",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "fk_unknown_deletions_site_users_processed_by_user_id",
                table: "unknown_deletions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_actions_site_users_site_user_id",
                table: "user_actions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_actions_unknown_deletions_unknown_deletion_id",
                table: "user_actions");

            migrationBuilder.DropForeignKey(
                name: "fk_user_actions_db_user_action_type_user_action_type_id",
                table: "user_actions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tags",
                table: "tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_roles",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_logs",
                table: "logs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_actions",
                table: "user_actions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_db_user_action_type",
                table: "user_action_types");

            migrationBuilder.DropPrimaryKey(
                name: "pk_unknown_deletions",
                table: "unknown_deletions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tag_statistics",
                table: "tag_statistics");

            migrationBuilder.DropPrimaryKey(
                name: "pk_site_users",
                table: "site_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_site_user_roles",
                table: "site_user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_site_user_role_audits",
                table: "site_user_role_audits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_seen_questions",
                table: "seen_questions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_db_meta_question_tag_tracking_status",
                table: "metag_question_tag_statuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_meta_tags",
                table: "meta_tags");

            migrationBuilder.DropPrimaryKey(
                name: "pk_meta_questions",
                table: "meta_questions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_meta_question_tags",
                table: "meta_question_tags");

            migrationBuilder.DropPrimaryKey(
                name: "pk_meta_question_tag_tracking_status_audits",
                table: "meta_question_tag_tracking_status_audits");

            migrationBuilder.DropPrimaryKey(
                name: "pk_db_meta_question_statistics",
                table: "meta_question_statistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_meta_question_meta_tags",
                table: "meta_question_meta_tags");

            migrationBuilder.DropPrimaryKey(
                name: "pk_meta_answers",
                table: "meta_answers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_db_meta_answer_statistics",
                table: "meta_answer_statistics");

            migrationBuilder.DropPrimaryKey(
                name: "pk_burnaki_follows",
                table: "burnaki_follows");

            migrationBuilder.RenameTable(
                name: "tags",
                newName: "Tags");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "logs",
                newName: "Logs");

            migrationBuilder.RenameTable(
                name: "user_actions",
                newName: "UserActions");

            migrationBuilder.RenameTable(
                name: "user_action_types",
                newName: "UserActionTypes");

            migrationBuilder.RenameTable(
                name: "unknown_deletions",
                newName: "UnknownDeletions");

            migrationBuilder.RenameTable(
                name: "tag_statistics",
                newName: "TagStatistics");

            migrationBuilder.RenameTable(
                name: "site_users",
                newName: "SiteUsers");

            migrationBuilder.RenameTable(
                name: "site_user_roles",
                newName: "SiteUserRoles");

            migrationBuilder.RenameTable(
                name: "site_user_role_audits",
                newName: "SiteUserRoleAudits");

            migrationBuilder.RenameTable(
                name: "seen_questions",
                newName: "SeenQuestions");

            migrationBuilder.RenameTable(
                name: "metag_question_tag_statuses",
                newName: "MetaQuestionTagStatuses");

            migrationBuilder.RenameTable(
                name: "meta_tags",
                newName: "MetaTags");

            migrationBuilder.RenameTable(
                name: "meta_questions",
                newName: "MetaQuestions");

            migrationBuilder.RenameTable(
                name: "meta_question_tags",
                newName: "MetaQuestionTags");

            migrationBuilder.RenameTable(
                name: "meta_question_tag_tracking_status_audits",
                newName: "MetaQuestionTagTrackingStatusAudits");

            migrationBuilder.RenameTable(
                name: "meta_question_statistics",
                newName: "MetaQuestionStatistics");

            migrationBuilder.RenameTable(
                name: "meta_question_meta_tags",
                newName: "MetaQuestionMetaTags");

            migrationBuilder.RenameTable(
                name: "meta_answers",
                newName: "MetaAnswers");

            migrationBuilder.RenameTable(
                name: "meta_answer_statistics",
                newName: "MetaAnswerStatistics");

            migrationBuilder.RenameTable(
                name: "burnaki_follows",
                newName: "BurnakiFollows");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Tags",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "synonym_of_tag_name",
                table: "Tags",
                newName: "SynonymOfTagName");

            migrationBuilder.RenameColumn(
                name: "number_of_questions",
                table: "Tags",
                newName: "NumberOfQuestions");

            migrationBuilder.RenameIndex(
                name: "IX_tags_synonym_of_tag_name",
                table: "Tags",
                newName: "IX_Tags_SynonymOfTagName");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Roles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "Logs",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "logger",
                table: "Logs",
                newName: "Logger");

            migrationBuilder.RenameColumn(
                name: "level",
                table: "Logs",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "exception",
                table: "Logs",
                newName: "Exception");

            migrationBuilder.RenameColumn(
                name: "callsite",
                table: "Logs",
                newName: "Callsite");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Logs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "time_logged",
                table: "Logs",
                newName: "TimeLogged");

            migrationBuilder.RenameIndex(
                name: "IX_logs_time_logged",
                table: "Logs",
                newName: "IX_Logs_TimeLogged");

            migrationBuilder.RenameColumn(
                name: "time",
                table: "UserActions",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "tag",
                table: "UserActions",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "UserActions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_action_type_id",
                table: "UserActions",
                newName: "UserActionTypeId");

            migrationBuilder.RenameColumn(
                name: "unknown_deletion_id",
                table: "UserActions",
                newName: "UnknownDeletionId");

            migrationBuilder.RenameColumn(
                name: "time_processed",
                table: "UserActions",
                newName: "TimeProcessed");

            migrationBuilder.RenameColumn(
                name: "site_user_id",
                table: "UserActions",
                newName: "SiteUserId");

            migrationBuilder.RenameColumn(
                name: "post_id",
                table: "UserActions",
                newName: "PostId");

            migrationBuilder.RenameIndex(
                name: "ix_user_actions_user_action_type_id",
                table: "UserActions",
                newName: "IX_UserActions_UserActionTypeId");

            migrationBuilder.RenameIndex(
                name: "ix_user_actions_unknown_deletion_id",
                table: "UserActions",
                newName: "IX_UserActions_UnknownDeletionId");

            migrationBuilder.RenameIndex(
                name: "ix_user_actions_site_user_id",
                table: "UserActions",
                newName: "IX_UserActions_SiteUserId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "UserActionTypes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "UserActionTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "time",
                table: "UnknownDeletions",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "tag",
                table: "UnknownDeletions",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "processed",
                table: "UnknownDeletions",
                newName: "Processed");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "UnknownDeletions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "processed_by_user_id",
                table: "UnknownDeletions",
                newName: "ProcessedByUserId");

            migrationBuilder.RenameColumn(
                name: "post_id",
                table: "UnknownDeletions",
                newName: "PostId");

            migrationBuilder.RenameIndex(
                name: "ix_unknown_deletions_processed_by_user_id",
                table: "UnknownDeletions",
                newName: "IX_UnknownDeletions_ProcessedByUserId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TagStatistics",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tag_name",
                table: "TagStatistics",
                newName: "TagName");

            migrationBuilder.RenameColumn(
                name: "question_count",
                table: "TagStatistics",
                newName: "QuestionCount");

            migrationBuilder.RenameColumn(
                name: "date_time",
                table: "TagStatistics",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "answer_count",
                table: "TagStatistics",
                newName: "AnswerCount");

            migrationBuilder.RenameIndex(
                name: "IX_tag_statistics_tag_name",
                table: "TagStatistics",
                newName: "IX_TagStatistics_TagName");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SiteUsers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "is_moderator",
                table: "SiteUsers",
                newName: "IsModerator");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "SiteUsers",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "enabled",
                table: "SiteUserRoles",
                newName: "Enabled");

            migrationBuilder.RenameColumn(
                name: "date_added",
                table: "SiteUserRoles",
                newName: "DateAdded");

            migrationBuilder.RenameColumn(
                name: "added_by_user_id",
                table: "SiteUserRoles",
                newName: "AddedByUserId");

            migrationBuilder.RenameColumn(
                name: "role_name",
                table: "SiteUserRoles",
                newName: "RoleName");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "SiteUserRoles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_site_user_roles_role_name",
                table: "SiteUserRoles",
                newName: "IX_SiteUserRoles_RoleName");

            migrationBuilder.RenameIndex(
                name: "IX_site_user_roles_added_by_user_id",
                table: "SiteUserRoles",
                newName: "IX_SiteUserRoles_AddedByUserId");

            migrationBuilder.RenameColumn(
                name: "added",
                table: "SiteUserRoleAudits",
                newName: "Added");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SiteUserRoleAudits",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "SiteUserRoleAudits",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "role_name",
                table: "SiteUserRoleAudits",
                newName: "RoleName");

            migrationBuilder.RenameColumn(
                name: "date_changed",
                table: "SiteUserRoleAudits",
                newName: "DateChanged");

            migrationBuilder.RenameColumn(
                name: "changed_by_user_id",
                table: "SiteUserRoleAudits",
                newName: "ChangedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_site_user_role_audits_user_id_role_name",
                table: "SiteUserRoleAudits",
                newName: "IX_SiteUserRoleAudits_UserId_RoleName");

            migrationBuilder.RenameIndex(
                name: "IX_site_user_role_audits_changed_by_user_id",
                table: "SiteUserRoleAudits",
                newName: "IX_SiteUserRoleAudits_ChangedByUserId");

            migrationBuilder.RenameColumn(
                name: "tag",
                table: "SeenQuestions",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SeenQuestions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "last_seen",
                table: "SeenQuestions",
                newName: "LastSeen");

            migrationBuilder.RenameIndex(
                name: "IX_seen_questions_tag",
                table: "SeenQuestions",
                newName: "IX_SeenQuestions_Tag");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "MetaQuestionTagStatuses",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MetaQuestionTagStatuses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "MetaTags",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "MetaQuestions",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "score",
                table: "MetaQuestions",
                newName: "Score");

            migrationBuilder.RenameColumn(
                name: "link",
                table: "MetaQuestions",
                newName: "Link");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "MetaQuestions",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MetaQuestions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "view_count",
                table: "MetaQuestions",
                newName: "ViewCount");

            migrationBuilder.RenameColumn(
                name: "last_seen",
                table: "MetaQuestions",
                newName: "LastSeen");

            migrationBuilder.RenameColumn(
                name: "featured_started",
                table: "MetaQuestions",
                newName: "FeaturedStarted");

            migrationBuilder.RenameColumn(
                name: "featured_ended",
                table: "MetaQuestions",
                newName: "FeaturedEnded");

            migrationBuilder.RenameColumn(
                name: "closed_date",
                table: "MetaQuestions",
                newName: "ClosedDate");

            migrationBuilder.RenameColumn(
                name: "close_reason",
                table: "MetaQuestions",
                newName: "CloseReason");

            migrationBuilder.RenameColumn(
                name: "burn_started",
                table: "MetaQuestions",
                newName: "BurnStarted");

            migrationBuilder.RenameColumn(
                name: "burn_ended",
                table: "MetaQuestions",
                newName: "BurnEnded");

            migrationBuilder.RenameColumn(
                name: "tracking_status_id",
                table: "MetaQuestionTags",
                newName: "TrackingStatusId");

            migrationBuilder.RenameColumn(
                name: "secondary_tag_name",
                table: "MetaQuestionTags",
                newName: "SecondaryTagName");

            migrationBuilder.RenameColumn(
                name: "tag_name",
                table: "MetaQuestionTags",
                newName: "TagName");

            migrationBuilder.RenameColumn(
                name: "meta_question_id",
                table: "MetaQuestionTags",
                newName: "MetaQuestionId");

            migrationBuilder.RenameIndex(
                name: "ix_meta_question_tags_tracking_status_id",
                table: "MetaQuestionTags",
                newName: "IX_MetaQuestionTags_TrackingStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_meta_question_tags_tag_name",
                table: "MetaQuestionTags",
                newName: "IX_MetaQuestionTags_TagName");

            migrationBuilder.RenameIndex(
                name: "IX_meta_question_tags_secondary_tag_name",
                table: "MetaQuestionTags",
                newName: "IX_MetaQuestionTags_SecondaryTagName");

            migrationBuilder.RenameColumn(
                name: "tag",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "time_changed",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "TimeChanged");

            migrationBuilder.RenameColumn(
                name: "previous_tracking_status_id",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "PreviousTrackingStatusId");

            migrationBuilder.RenameColumn(
                name: "new_tracking_status_id",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "NewTrackingStatusId");

            migrationBuilder.RenameColumn(
                name: "meta_question_id",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "MetaQuestionId");

            migrationBuilder.RenameColumn(
                name: "changed_by_user_id",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "ChangedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_meta_question_tag_tracking_status_audits_meta_question_id_t~",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "IX_MetaQuestionTagTrackingStatusAudits_MetaQuestionId_Tag");

            migrationBuilder.RenameIndex(
                name: "IX_meta_question_tag_tracking_status_audits_previous_tracking_~",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "IX_MetaQuestionTagTrackingStatusAudits_PreviousTrackingStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_meta_question_tag_tracking_status_audits_new_tracking_statu~",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "IX_MetaQuestionTagTrackingStatusAudits_NewTrackingStatusId");

            migrationBuilder.RenameIndex(
                name: "ix_meta_question_tag_tracking_status_audits_changed_by_user_id",
                table: "MetaQuestionTagTrackingStatusAudits",
                newName: "IX_MetaQuestionTagTrackingStatusAudits_ChangedByUserId");

            migrationBuilder.RenameColumn(
                name: "score",
                table: "MetaQuestionStatistics",
                newName: "Score");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MetaQuestionStatistics",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "view_count",
                table: "MetaQuestionStatistics",
                newName: "ViewCount");

            migrationBuilder.RenameColumn(
                name: "meta_question_id",
                table: "MetaQuestionStatistics",
                newName: "MetaQuestionId");

            migrationBuilder.RenameColumn(
                name: "date_time",
                table: "MetaQuestionStatistics",
                newName: "DateTime");

            migrationBuilder.RenameIndex(
                name: "ix_db_meta_question_statistics_meta_question_id",
                table: "MetaQuestionStatistics",
                newName: "IX_MetaQuestionStatistics_MetaQuestionId");

            migrationBuilder.RenameColumn(
                name: "tag_name",
                table: "MetaQuestionMetaTags",
                newName: "TagName");

            migrationBuilder.RenameColumn(
                name: "meta_question_id",
                table: "MetaQuestionMetaTags",
                newName: "MetaQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_meta_question_meta_tags_tag_name",
                table: "MetaQuestionMetaTags",
                newName: "IX_MetaQuestionMetaTags_TagName");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "MetaAnswers",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MetaAnswers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "meta_question_id",
                table: "MetaAnswers",
                newName: "MetaQuestionId");

            migrationBuilder.RenameColumn(
                name: "last_seen",
                table: "MetaAnswers",
                newName: "LastSeen");

            migrationBuilder.RenameIndex(
                name: "ix_meta_answers_meta_question_id",
                table: "MetaAnswers",
                newName: "IX_MetaAnswers_MetaQuestionId");

            migrationBuilder.RenameColumn(
                name: "score",
                table: "MetaAnswerStatistics",
                newName: "Score");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MetaAnswerStatistics",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "meta_answer_id",
                table: "MetaAnswerStatistics",
                newName: "MetaAnswerId");

            migrationBuilder.RenameColumn(
                name: "date_time",
                table: "MetaAnswerStatistics",
                newName: "DateTime");

            migrationBuilder.RenameIndex(
                name: "ix_db_meta_answer_statistics_meta_answer_id",
                table: "MetaAnswerStatistics",
                newName: "IX_MetaAnswerStatistics_MetaAnswerId");

            migrationBuilder.RenameColumn(
                name: "tag",
                table: "BurnakiFollows",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "BurnakiFollows",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "room_id",
                table: "BurnakiFollows",
                newName: "RoomId");

            migrationBuilder.RenameColumn(
                name: "follow_started",
                table: "BurnakiFollows",
                newName: "FollowStarted");

            migrationBuilder.RenameColumn(
                name: "follow_ended",
                table: "BurnakiFollows",
                newName: "FollowEnded");

            migrationBuilder.RenameColumn(
                name: "burnaki_id",
                table: "BurnakiFollows",
                newName: "BurnakiId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Logs",
                table: "Logs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActions",
                table: "UserActions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActionTypes",
                table: "UserActionTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UnknownDeletions",
                table: "UnknownDeletions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TagStatistics",
                table: "TagStatistics",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteUsers",
                table: "SiteUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteUserRoles",
                table: "SiteUserRoles",
                columns: new[] { "UserId", "RoleName" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteUserRoleAudits",
                table: "SiteUserRoleAudits",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SeenQuestions",
                table: "SeenQuestions",
                columns: new[] { "Id", "Tag" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaQuestionTagStatuses",
                table: "MetaQuestionTagStatuses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaTags",
                table: "MetaTags",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaQuestions",
                table: "MetaQuestions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaQuestionTags",
                table: "MetaQuestionTags",
                columns: new[] { "MetaQuestionId", "TagName" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaQuestionTagTrackingStatusAudits",
                table: "MetaQuestionTagTrackingStatusAudits",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaQuestionStatistics",
                table: "MetaQuestionStatistics",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaQuestionMetaTags",
                table: "MetaQuestionMetaTags",
                columns: new[] { "MetaQuestionId", "TagName" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaAnswers",
                table: "MetaAnswers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaAnswerStatistics",
                table: "MetaAnswerStatistics",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BurnakiFollows",
                table: "BurnakiFollows",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MetaAnswers_MetaQuestions_MetaQuestionId",
                table: "MetaAnswers",
                column: "MetaQuestionId",
                principalTable: "MetaQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaAnswerStatistics_MetaAnswers_MetaAnswerId",
                table: "MetaAnswerStatistics",
                column: "MetaAnswerId",
                principalTable: "MetaAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionMetaTags_MetaQuestions_MetaQuestionId",
                table: "MetaQuestionMetaTags",
                column: "MetaQuestionId",
                principalTable: "MetaQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionMetaTags_MetaTags_TagName",
                table: "MetaQuestionMetaTags",
                column: "TagName",
                principalTable: "MetaTags",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionStatistics_MetaQuestions_MetaQuestionId",
                table: "MetaQuestionStatistics",
                column: "MetaQuestionId",
                principalTable: "MetaQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestions_MetaQuestionId",
                table: "MetaQuestionTags",
                column: "MetaQuestionId",
                principalTable: "MetaQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTags_Tags_SecondaryTagName",
                table: "MetaQuestionTags",
                column: "SecondaryTagName",
                principalTable: "Tags",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTags_Tags_TagName",
                table: "MetaQuestionTags",
                column: "TagName",
                principalTable: "Tags",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestionTagStatuses_TrackingStatusId",
                table: "MetaQuestionTags",
                column: "TrackingStatusId",
                principalTable: "MetaQuestionTagStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_SiteUsers_ChangedByUser~",
                table: "MetaQuestionTagTrackingStatusAudits",
                column: "ChangedByUserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestions_MetaQuest~",
                table: "MetaQuestionTagTrackingStatusAudits",
                column: "MetaQuestionId",
                principalTable: "MetaQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTagStatuses~",
                table: "MetaQuestionTagTrackingStatusAudits",
                column: "NewTrackingStatusId",
                principalTable: "MetaQuestionTagStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTagStatuse~1",
                table: "MetaQuestionTagTrackingStatusAudits",
                column: "PreviousTrackingStatusId",
                principalTable: "MetaQuestionTagStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTags_MetaQu~",
                table: "MetaQuestionTagTrackingStatusAudits",
                columns: new[] { "MetaQuestionId", "Tag" },
                principalTable: "MetaQuestionTags",
                principalColumns: new[] { "MetaQuestionId", "TagName" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUserRoleAudits_SiteUsers_ChangedByUserId",
                table: "SiteUserRoleAudits",
                column: "ChangedByUserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUserRoleAudits_SiteUsers_UserId",
                table: "SiteUserRoleAudits",
                column: "UserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUserRoleAudits_SiteUserRoles_UserId_RoleName",
                table: "SiteUserRoleAudits",
                columns: new[] { "UserId", "RoleName" },
                principalTable: "SiteUserRoles",
                principalColumns: new[] { "UserId", "RoleName" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUserRoles_SiteUsers_AddedByUserId",
                table: "SiteUserRoles",
                column: "AddedByUserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUserRoles_Roles_RoleName",
                table: "SiteUserRoles",
                column: "RoleName",
                principalTable: "Roles",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUserRoles_SiteUsers_UserId",
                table: "SiteUserRoles",
                column: "UserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tags_SynonymOfTagName",
                table: "Tags",
                column: "SynonymOfTagName",
                principalTable: "Tags",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TagStatistics_Tags_TagName",
                table: "TagStatistics",
                column: "TagName",
                principalTable: "Tags",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownDeletions_SiteUsers_ProcessedByUserId",
                table: "UnknownDeletions",
                column: "ProcessedByUserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActions_SiteUsers_SiteUserId",
                table: "UserActions",
                column: "SiteUserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActions_UnknownDeletions_UnknownDeletionId",
                table: "UserActions",
                column: "UnknownDeletionId",
                principalTable: "UnknownDeletions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActions_UserActionTypes_UserActionTypeId",
                table: "UserActions",
                column: "UserActionTypeId",
                principalTable: "UserActionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
