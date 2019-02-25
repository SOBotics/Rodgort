using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class AddZombieTagsView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE MATERIALIZED VIEW zombie_tags as (
	select 
		tag_name as tag_name,
		date_time time_revived
	 from (
		select 
			ts.tag_name,
		    ts.date_time,
			case 
				when lag(question_count) over (partition by ts.tag_name order by ts.date_time) = 0 
	            and not lag(is_synonym) over (partition by ts.tag_name order by ts.date_time)
	            and question_count > 0 then true
				else false
			end 
			as revived
		from tag_statistics ts
		inner join meta_question_tags mqt on mqt.tag_name = ts.tag_name and mqt.tracking_status_id = 2
	) innerQuery
	where innerQuery.revived
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP MATERIALIZED VIEW zombie_tags;
");
        }
    }
}
