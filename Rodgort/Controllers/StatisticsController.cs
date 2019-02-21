using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Services;
using Rodgort.Utilities;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class StatisticsController : Controller
    {
        private readonly RodgortContext _context;
        private readonly DateService _dateService;
        
        public StatisticsController(RodgortContext context, DateService dateService)
        {
            _context = context;
            _dateService = dateService;
        }
        
        [HttpGet]
        public object Get()
        {
            var burnRequests = _context.MetaQuestions.Where(mq => mq.MetaQuestionMetaTags.Any(mqtmt => DbMetaTag.RequestTypes.Contains(mqtmt.TagName)));

            var burninationRequests = burnRequests.Count();

            var currentBurns = burnRequests.Count(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_PLANNED));
            var proposedBurns = burnRequests.Count(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_FEATURED));

            var burninationRequestsWithTracked = burnRequests.Count(mq => mq.MetaQuestionTags.Any(mqt => 
                mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE));
            var burninationRequestsRequireTrackingApproval = burnRequests.Count(mq => mq.MetaQuestionTags.Any(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL));
            
            var declinedRequests = burnRequests.Count(mq => mq.MetaQuestionMetaTags.Any(mtqm => mtqm.TagName == DbMetaTag.STATUS_DECLINED));
            var completedRequests = burnRequests.Count(mq => mq.MetaQuestionMetaTags.Any(mtqm => mtqm.TagName == DbMetaTag.STATUS_COMPLETED));

            var unknownDeletions = _context.UnknownDeletions.Count(ud => !ud.Processed.HasValue);

            var completedRequestsWithQuestions = burnRequests.Count(mq => 
                mq.MetaQuestionMetaTags.Any(mtqm => mtqm.TagName == DbMetaTag.STATUS_COMPLETED)
                && mq.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED).Any(mqt => mqt.Tag.NumberOfQuestions > 0)
            );

            var statusTags = DbMetaTag.StatusFlags;
            var noStatusNoQuestions = burnRequests.Count(mq => 
                !mq.MetaQuestionMetaTags.Any(mqa => statusTags.Contains(mqa.TagName))
                && !mq.ClosedDate.HasValue
                && mq.MetaQuestionTags.Any(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED)
                && mq.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED).All(mqt => mqt.Tag.NumberOfQuestions <= 0)
            );

            var filteredTags = _context.Tags.Where(t => t.MetaQuestionTags.Any(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED));
            var tagCount = filteredTags.Count();
            var tagsNoQuestions = filteredTags.Count(t => t.NumberOfQuestions == 0);
            var synonomisedTags = filteredTags.Count(t => t.SynonymOf != null);

            var tagsQuestionsOnCompleted = filteredTags.Count(t =>
                t.NumberOfQuestions > 0
                && t.MetaQuestionTags.Any(mqt =>
                    (mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED
                    || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE)
                    && mqt.MetaQuestion.MetaQuestionMetaTags.Any(mqma => mqma.TagName == DbMetaTag.STATUS_COMPLETED))
            );

            return new
            {
                Requests = new
                {
                    Total = burninationRequests,

                    CurrentBurns = currentBurns,
                    ProposedBurns = proposedBurns,

                    WithTrackedTags = burninationRequestsWithTracked,
                    RequireTrackingApproval = burninationRequestsRequireTrackingApproval,
                    
                    Declined = declinedRequests,
                    Completed = completedRequests,

                    CompletedWithQuestionsLeft = completedRequestsWithQuestions,
                    NoStatusButCompleted = noStatusNoQuestions
                }, 
                Tags = new
                {
                    Total = tagCount,
                    noQuestions = tagsNoQuestions,
                    synonymised = synonomisedTags,
                    hasQuestionsAndAttachedToCompletedRequest = tagsQuestionsOnCompleted
                },
                Admin = new
                {
                    UnknownDeletions = unknownDeletions
                }
            };
        }

        [HttpGet("Leaderboard/Current")]
        public object CurrentLeaderboard()
        {
            return GenerateBurnsData(
                _context.MetaQuestions
                    .Where(mq => mq.MetaQuestionMetaTags.Any(mqtmt => DbMetaTag.RequestTypes.Contains(mqtmt.TagName)))
                    .Where(mq => mq.MetaQuestionMetaTags.Any(mqmt => mqmt.TagName == DbMetaTag.STATUS_PLANNED))
            );
        }

        [HttpGet("Leaderboard")]
        public object LeaderboardForId(int metaQuestionId)
        {
            return GenerateBurnsData(
                _context.MetaQuestions
                    .Where(mq => mq.MetaQuestionMetaTags.Any(mqtmt => DbMetaTag.RequestTypes.Contains(mqtmt.TagName)))
                    .Where(mq => mq.Id == metaQuestionId)
            );
        }

        private object GenerateBurnsData(IQueryable<DbMetaQuestion> query)
        {
            var isRoomOwner = User.HasClaim(DbRole.TROGDOR_ROOM_OWNER);
            var now = _dateService.UtcNow;
            var monthAgo = now.AddMonths(-1);
            var inAnHour = now.AddHours(1);

            var burnsData = query
                .Select(mq => new
                {
                    mq.FeaturedStarted,
                    mq.FeaturedEnded,
                    mq.BurnStarted,
                    mq.BurnEnded,
                    StartTime = mq.FeaturedStarted ?? mq.FeaturedEnded ?? mq.BurnStarted ?? mq.BurnEnded ?? monthAgo,
                    EndTime = mq.FeaturedEnded.HasValue && !mq.BurnStarted.HasValue
                                ? mq.FeaturedEnded.Value
                                : mq.BurnStarted.HasValue && mq.BurnEnded.HasValue
                                    ? mq.BurnEnded.Value
                                    : inAnHour,
                    mq.Title,
                    mq.Link,
                    BurningTags = mq.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED || mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED_ELSEWHERE)
                }).Select(mq => new
                {
                    mq.FeaturedStarted,
                    mq.FeaturedEnded,
                    mq.BurnStarted,
                    mq.BurnEnded,
                    mq.StartTime,
                    mq.EndTime,
                    mq.Title,
                    mq.Link,

                    BurningTags = mq.BurningTags.Select(mqt =>
                        new
                        {
                            Tag = mqt.TagName,
                            mqt.Tag.NumberOfQuestions,
                            QuestionCountOverTime = mqt.Tag.Statistics.Where(s => s.DateTime > mq.StartTime).Select(s => new { s.DateTime, s.QuestionCount }).OrderBy(s => s.DateTime).ToList(),
                        }).ToList()
                })
                .ToList();

            var res = new
            {
                Burns = burnsData.Select(b => new
                {
                    MetaQuestionTitle = b.Title,
                    MetaQuestionLink = b.Link,
                    Tags = b.BurningTags.Select(bt =>
                    {
                        return new
                        {
                            bt.Tag,
                            bt.NumberOfQuestions,
                            QuestionCountOverTime = bt.QuestionCountOverTime.Where(q => q.DateTime <= b.EndTime).ToList(),

                            ClosuresOverTime = LoadClosuresOverTimeData(b.StartTime, b.EndTime, bt.Tag),
                            DeletionsOverTime = LoadDeletionsOverTimeData(b.StartTime, b.EndTime, bt.Tag),
                            RetagsOverTime = LoadRetagsOverTimeData(b.StartTime, b.EndTime, bt.Tag),
                            RoombasOverTime = LoadRoombasOverTimeData(b.StartTime, b.EndTime, bt.Tag),

                            Overtime = LoadOverTimeData(b.StartTime, b.EndTime, bt.Tag),

                            UserTotals = LoadUserTotalsData(b.StartTime, b.EndTime, b.BurnStarted ?? b.StartTime, isRoomOwner, bt.Tag),

                            UserGrandTotals = LoadUserGrandTotalsData(b.StartTime, b.EndTime, b.BurnStarted ?? b.StartTime, isRoomOwner, bt.Tag)
                        };
                    }),

                    b.FeaturedStarted,
                    b.FeaturedEnded,
                    b.BurnStarted,
                    b.BurnEnded
                }).ToList()
            };
            return res;
        }

        private List<ActionOverTimeQuery> LoadClosuresOverTimeData(DateTime startTime, DateTime endTime, string tag)
        {
            return _context
                .Database.GetDbConnection()
                .Query<ActionOverTimeQuery>(@"
with 
hours as (
	select generate_series(date_trunc('day', @startTime), date_trunc('day', @endTime), interval '1 day' hour) as hour
)


select 
	date_trunc('day', time) as Date,
	MAX(running_total) as Total
from (
	select 
	*,
	SUM(direction) over (order by time) as running_total
	from (
		select 
			time,
			direction
		from (
			select 
			distinct
			post_id,
			user_action_type_id,
			time,
			case 
				when user_action_type_id = 3 then 1
				when user_action_type_id = 4 then -1
				else 0
			end as direction
			from user_actions
			where 
				time < @endTime
				and tag = @tag
				and user_action_type_id in (3, 4)
		) innerQuery
		
		union all 
		select 
		hour as time,
		0 as direction
		from hours
	) innerQuery
) innerQuery
where date_trunc('day', time) > @startTime 
group by date_trunc('day', time)
order by date_trunc('day', time)", new
                {
                    startTime,
                    endTime,
                    tag
                })
                .ToList();
        }

        private List<ActionOverTimeQuery> LoadDeletionsOverTimeData(DateTime startTime, DateTime endTime, string tag)
        {
            return _context
                .Database.GetDbConnection()
                .Query<ActionOverTimeQuery>(@"
with 
hours as (
	select generate_series(date_trunc('day', @startTime), date_trunc('day', @endTime), interval '1 day' hour) as hour
)

select 
	date_trunc('day', time) as Date,
	MAX(running_total) as Total
from (
	select 
	*,
	SUM(direction) over (order by time) as running_total
	from (
		select 
			time,
			direction
		from (
			select 
			distinct
			post_id,
			user_action_type_id,
			time,
			case 
				when user_action_type_id = 5 then 1
				when user_action_type_id = 6 then -1
				else 0
			end as direction
			from user_actions
			where 
				time > @startTime and time < @endTime
				and tag = @tag
				and user_action_type_id in (5, 6)
		) innerQuery
		
		union all 
		select 
		hour as time,
		0 as direction
		from hours
	) innerQuery
) innerQuery
where date_trunc('day', time) > @startTime 
group by date_trunc('day', time)
order by date_trunc('day', time)", new
                {
                    startTime,
                    endTime,
                    tag
                })
                .ToList();
        }

        private List<ActionOverTimeQuery> LoadRetagsOverTimeData(DateTime startTime, DateTime endTime, string tag)
        {
            return _context
                .Database.GetDbConnection()
                .Query<ActionOverTimeQuery>(@"
with 
hours as (
	select generate_series(date_trunc('day', @startTime), date_trunc('day', @endTime), interval '1 day' hour) as hour
)

select 
	date_trunc('day', time) as Date,
	MAX(running_total) as Total
from (
	select 
	*,
	SUM(direction) over (order by time) as running_total
	from (
		select 
			time,
			direction
		from (
			select 
			distinct
			post_id,
			user_action_type_id,
			time,
			case 
				when user_action_type_id = 1 then 1
				when user_action_type_id = 2 then -1
				else 0
			end as direction
			from user_actions
			where 
				time > @startTime and time < @endTime
				and tag = @tag
				and user_action_type_id in (1, 2)
		) innerQuery
		
		union all 
		select 
		hour as time,
		0 as direction
		from hours
	) innerQuery
) innerQuery
where date_trunc('day', time) > @startTime 
group by date_trunc('day', time)
order by date_trunc('day', time)", new
                {
                    startTime,
                    endTime,
                    tag
                })
                .ToList();
        }

        private List<ActionOverTimeQuery> LoadRoombasOverTimeData(DateTime startTime, DateTime endTime, string tag)
        {
            return _context
                .Database.GetDbConnection()
                .Query<ActionOverTimeQuery>(@"
with 
hours as (
	select generate_series(date_trunc('day', @startTime), date_trunc('day', @endTime), interval '1 day' hour) as hour
)

select 
	date_trunc('day', time) as Date,
	MAX(running_total) as Total
from (
	select 
	*,
	SUM(direction) over (order by time) as running_total
	from (
		select 
			time,
			direction
		from (
			select 
			distinct
			post_id,
			user_action_type_id,
			time,
			case 
				when user_action_type_id = 5 and site_user_id = -1 then 1
				when user_action_type_id = 6 then -1
				else 0
			end as direction
			from user_actions
			where 
				time > @startTime and time < @endTime
				and tag = @tag
				and user_action_type_id in (5, 6)
		) innerQuery
		
		union all 
		select 
		hour as time,
		0 as direction
		from hours
	) innerQuery
) innerQuery
where date_trunc('day', time) > @startTime 
group by date_trunc('day', time)
order by date_trunc('day', time)", new
                {
                    startTime,
                    endTime,
                    tag
                })
                .ToList();
        }


        private object LoadOverTimeData(DateTime startTime, DateTime endTime, string tag)
        {
            return _context
                .Database.GetDbConnection()
                .Query<OverTimeQuery>(@"
with 
hours as (
	select generate_series(date_trunc('day', @StartTime), date_trunc('day', @EndTime), interval '1 day' hour) as hour
),
siteUsers as (
	select * from site_users
	WHERE site_users.id in (
		SELECT 
			site_user_id 
		FROM 
			user_actions
		WHERE site_user_id != -1
			and tag = @Tag 
			and time > @StartTime 
			and time < @EndTime
		GROUP BY 
			site_user_id
		ORDER BY 
			COUNT(*) DESC
		LIMIT 10
	)
)

select distinct
user_id as UserId,
display_name as DisplayName,
is_moderator as IsModerator,
hour as Hour,
SUM(hour_total) over (partition by user_id order by hour range between unbounded preceding and current row) as RunningTotal
from 
(
	select 
		siteUsers.id as user_id,
		siteUsers.display_name,
		siteUsers.is_moderator,
		date_trunc('day', user_actions.time) as hour,
		SUM(case when user_actions.id is null then 0 else 1 end) as hour_total
	from 
		siteUsers
	inner join user_actions 
		on user_actions.site_user_id = siteUsers.id
		and user_actions.tag = @Tag and user_actions.time > @StartTime and user_actions.time < @EndTime
	group by 
		siteUsers.id,
		siteUsers.display_name,
		siteUsers,is_moderator,
		date_trunc('day', user_actions.time)

    UNION ALL

    select 
	siteUsers.id as user_id,
	siteUsers.display_name,
	siteUsers.is_moderator,
	hour,
    0 as hour_total
    from siteUsers
    left join lateral (select hour as hour from hours) h on true
) hourlyQuery;", new {
                    StartTime = startTime,
                    EndTime = endTime,
                    Tag = tag
                })
                .GroupBy(g => new { g.DisplayName, g.IsModerator, g.UserId })
                .Select(g => new
                {
                    UserName = g.Key.DisplayName,
                    g.Key.IsModerator,
                    Times = g.OrderBy(gg => gg.Hour).Select(gg => new
                    {
                        Date = gg.Hour,
                        Total = gg.RunningTotal
                    })
                })
                .OrderByDescending(g => g.Times.Max(tt => tt.Total))
                .ToList();
        }

        private List<UserTotalsData> LoadUserTotalsData(DateTime startTime, DateTime endTime, DateTime burnStart, bool isTrogdorRoomOwner, string tag)
        {
            return _context
                .Database.GetDbConnection()
                .Query<UserTotalsData>(@"
select 
	site_users.id as UserId,
	site_users.display_name as UserName,
	site_users.is_moderator as IsModerator,
	user_action_types.name as Type,
	COUNT(distinct user_actions.post_id) as Total
from user_actions
inner join site_users on user_actions.site_user_id = site_users.id
inner join user_action_types on user_actions.user_action_type_id = user_action_types.id
where (tag = @tag and time > @startTime and time < @endTime)
and (@isTrogdorRoomOwner or time > @burnStart) 
and site_users.id > 0
group by 
	site_users.id,
	site_users.display_name,
	site_users.is_moderator,
	user_action_types.name
order by COUNT(distinct user_actions.post_id) desc", new
                {
                    startTime,
                    endTime,
                    burnStart,
                    isTrogdorRoomOwner,
                    tag
                })
                .ToList();
        }

        private List<UserTotalsData> LoadUserGrandTotalsData(DateTime startTime, DateTime endTime, DateTime burnStart, bool isTrogdorRoomOwner, string tag)
        {
            return _context
                .Database.GetDbConnection()
                .Query<UserTotalsData>(@"
select 
	site_users.id as UserId,
	site_users.display_name as UserName,
	site_users.is_moderator as IsModerator,
	COUNT(distinct user_actions.post_id) as Total
from user_actions
inner join site_users on user_actions.site_user_id = site_users.id
where (tag = @tag and time > @startTime and time < @endTime)
and (@isTrogdorRoomOwner or time > @burnStart) 
and site_users.id > 0
group by 
	site_users.id,
	site_users.display_name,
	site_users.is_moderator
order by COUNT(distinct user_actions.post_id) desc", new
                {
                    startTime,
                    endTime,
                    burnStart,
                    isTrogdorRoomOwner,
                    tag
                })
                .ToList();
        }

        private class ActionOverTimeQuery
        {
            public DateTime Date { get; set; }
            public int Total { get; set; }
        }

        private class OverTimeQuery
        {
            public int UserId { get; set; }
            public string DisplayName { get; set; }
            public bool IsModerator { get; set; }
            public DateTime Hour { get; set; }
            public int RunningTotal { get; set; }
        }

        private class UserTotalsData
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public bool IsModerator { get; set; }
            public string Type { get; set; }
            public int Total { get; set; }
        }

        private class UserGrandTotalsData
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public bool IsModerator { get; set; }
            public int Total { get; set; }
        }
    }
}