using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class AddUserStatisticsView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW user_statistics as (
    select 
	    inner_query.*,
	    count(distinct meta_question_tag_tracking_status_audits.id) as triaged_tags,
	    count(distinct meta_question_tag_tracking_status_audits.meta_question_id) as triaged_questions
    from
    (
	    select 
		    site_users.id as user_id, 
		    display_name, 
		    is_moderator, 
		    count(distinct user_actions.post_id) as num_burn_actions
	    FROM 
		    site_users
	    inner join user_actions on user_actions.site_user_id = site_users.id
	    inner join meta_question_tags on meta_question_tags.tag_name = user_actions.tag
	    inner join meta_questions ON meta_questions.id = meta_question_tags.meta_question_id
	    where 
		    meta_question_tags.tracking_status_id = 2
		    and meta_questions.burn_started IS NOT NULL
		    and user_actions.time > meta_questions.burn_started 
	    group by 
		    site_users.id, display_name, is_moderator
    ) inner_query
    left join meta_question_tag_tracking_status_audits on meta_question_tag_tracking_status_audits.changed_by_user_id = inner_query.user_id
    group by 
	    inner_query.user_id, inner_query.display_name, inner_query.is_moderator, inner_query.num_burn_actions
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP MATERIALIZED VIEW user_statistics;
");
        }
    }
}
