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

            var trackedCompletedRequests = burnRequests.Count(mq => mq.BurnStarted.HasValue);

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

            var numZombies = _context.ZombieTagsView.Where(z => z.Tag.NumberOfQuestions > 0).Select(z => z.TagName).Distinct().Count();

//            var numZombies = _context.Database.GetDbConnection().Query<ZombieQuery>(@"
//select COUNT(*) as ZombieCount
//FROM (
//	select
//	distinct no_questions.tag_name  
//	from 
//    tags
//    inner join tag_statistics no_questions on no_questions.tag_name = tags.name
//  	inner join meta_question_tags mqt on mqt.tag_name = tags.name and mqt.tracking_status_id = 2  
//	where exists (
//		select NULL FROM
//		tag_statistics has_questions where has_questions.tag_name = tags.name and has_questions.date_time > no_questions.date_time and has_questions.question_count > 0
//	)
//	and no_questions.question_count = 0 and not no_questions.is_synonym
//    and tags.number_of_questions > 0
//) innerQuery
//").First().ZombieCount;

            var numUsers = _context.SiteUsers.Count();

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
                    TrackedCompleted = trackedCompletedRequests,

                    CompletedWithQuestionsLeft = completedRequestsWithQuestions,
                    NoStatusButCompleted = noStatusNoQuestions,
                }, 
                Tags = new
                {
                    Total = tagCount,
                    NoQuestions = tagsNoQuestions,
                    Synonymised = synonomisedTags,
                    HasQuestionsAndAttachedToCompletedRequest = tagsQuestionsOnCompleted,
                    ZombieCount = numZombies,
                },
                Admin = new
                {
                    UnknownDeletions = unknownDeletions
                },
                Users = new
                {
                    TotalUsers = numUsers
                }
            };
        }

        [HttpGet("TrackedBurns")]
        public object TrackedBurns()
        {
            return _context.MetaQuestions.Where(mq => mq.BurnStarted.HasValue)
                .Select(mq => new
                {
                    mq.Id,
                    mq.Title,
                    Tags = mq.MetaQuestionTags.Where(mqt => mqt.TrackingStatusId == DbMetaQuestionTagTrackingStatus.TRACKED).Select(mqt => mqt.TagName),
                    mq.BurnStarted,
                    mq.BurnEnded
                })
                .OrderByDescending(mq => mq.BurnStarted)
                .ToList();
        }


        [HttpGet("Leaderboard/All")]
        public object AllLeaderboards()
        {
            return GenerateBurnsData(_context.MetaQuestions.Where(mq => mq.BurnStarted.HasValue));
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
                    mq.Id,
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
                    mq.Id,

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
                    MetaQuestionId = b.Id,
                    MetaQuestionTitle = b.Title,
                    MetaQuestionLink = b.Link,
                    Tags = b.BurningTags.Select(bt =>
                    {
                        var breakdownSize = (b.EndTime - b.StartTime).TotalDays < 100
                            ? "hour"
                            : "day";

                        var breakdownInterval = breakdownSize == "hour" ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1);

                        return new
                        {
                            bt.Tag,
                            bt.NumberOfQuestions,
                            QuestionCountOverTime = bt.QuestionCountOverTime.Where(q => q.DateTime <= b.EndTime).ToList(),

                            ClosuresOverTime = LoadClosuresOverTimeData(b.StartTime, b.EndTime, bt.Tag, breakdownSize, breakdownInterval),
                            DeletionsOverTime = LoadDeletionsOverTimeData(b.StartTime, b.EndTime, bt.Tag, breakdownSize, breakdownInterval),
                            RetagsOverTime = LoadRetagsOverTimeData(b.StartTime, b.EndTime, bt.Tag, breakdownSize, breakdownInterval),
                            RoombasOverTime = LoadRoombasOverTimeData(b.StartTime, b.EndTime, bt.Tag, breakdownSize, breakdownInterval),

                            Overtime = LoadOverTimeData(isRoomOwner ? b.StartTime : (b.BurnStarted ?? b.BurnEnded ?? inAnHour), b.EndTime, bt.Tag, breakdownSize, breakdownInterval),

                            UserTotals = LoadUserTotalsData(isRoomOwner ? b.StartTime : (b.BurnStarted ?? b.BurnEnded ?? inAnHour), b.EndTime, b.BurnStarted ?? b.StartTime, bt.Tag),

                            UserGrandTotals = LoadUserGrandTotalsData(isRoomOwner ? b.StartTime : (b.BurnStarted ?? b.BurnEnded ?? inAnHour), b.EndTime, b.BurnStarted ?? b.StartTime, bt.Tag)
                        };
                    }),

                    b.FeaturedStarted,
                    b.FeaturedEnded,
                    b.BurnStarted,
                    b.BurnEnded
                })
                .OrderByDescending(b => b.BurnStarted)
                .ToList()
            };
            return res;
        }

        private List<ActionOverTimeQuery> LoadClosuresOverTimeData(DateTime startTime, DateTime endTime, string tag, string breakdownSize, TimeSpan breakdownInterval)
        {
            return _context
                .Database.GetDbConnection()
                .Query<ActionOverTimeQuery>(@"
with 
times as (
	select generate_series(date_trunc(@breakdownSize, @startTime), date_trunc(@breakdownSize, @endTime), @breakdownInterval) as time
)

select 
	date_trunc(@breakdownSize, time) as Date,
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
		time,
		0 as direction
		from times
	) innerQuery
) innerQuery
where date_trunc(@breakdownSize, time) > @startTime 
group by date_trunc(@breakdownSize, time)
order by date_trunc(@breakdownSize, time)", new
                {
                    startTime,
                    endTime,
                    tag,
                    breakdownSize,
                    breakdownInterval
                })
                .ToList();
        }

        private List<ActionOverTimeQuery> LoadDeletionsOverTimeData(DateTime startTime, DateTime endTime, string tag, string breakdownSize, TimeSpan breakdownInterval)
        {
            return _context
                .Database.GetDbConnection()
                .Query<ActionOverTimeQuery>(@"
with 
times as (
	select generate_series(date_trunc(@breakdownSize, @startTime), date_trunc(@breakdownSize, @endTime), @breakdownInterval) as time
)

select 
	date_trunc(@breakdownSize, time) as Date,
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
		time,
		0 as direction
		from times
	) innerQuery
) innerQuery
where date_trunc(@breakdownSize, time) > @startTime 
group by date_trunc(@breakdownSize, time)
order by date_trunc(@breakdownSize, time)", new
                {
                    startTime,
                    endTime,
                    tag,
                    breakdownSize,
                    breakdownInterval
                })
                .ToList();
        }

        private List<ActionOverTimeQuery> LoadRetagsOverTimeData(DateTime startTime, DateTime endTime, string tag, string breakdownSize, TimeSpan breakdownInterval)
        {
            return _context
                .Database.GetDbConnection()
                .Query<ActionOverTimeQuery>(@"
with 
times as (
	select generate_series(date_trunc(@breakdownSize, @startTime), date_trunc(@breakdownSize, @endTime), @breakdownInterval) as time
)

select 
	date_trunc(@breakdownSize, time) as Date,
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
		time,
		0 as direction
		from times
	) innerQuery
) innerQuery
where date_trunc(@breakdownSize, time) > @startTime 
group by date_trunc(@breakdownSize, time)
order by date_trunc(@breakdownSize, time)", new
                {
                    startTime,
                    endTime,
                    tag,
                    breakdownSize,
                    breakdownInterval
                })
                .ToList();
        }

        private List<ActionOverTimeQuery> LoadRoombasOverTimeData(DateTime startTime, DateTime endTime, string tag, string breakdownSize, TimeSpan breakdownInterval)
        {
            return _context
                .Database.GetDbConnection()
                .Query<ActionOverTimeQuery>(@"
with 
times as (
	select generate_series(date_trunc(@breakdownSize, @startTime), date_trunc(@breakdownSize, @endTime), @breakdownInterval) as time
)

select 
	date_trunc(@breakdownSize, time) as Date,
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
		time,
		0 as direction
		from times
	) innerQuery
) innerQuery
where date_trunc(@breakdownSize, time) > @startTime 
group by date_trunc(@breakdownSize, time)
order by date_trunc(@breakdownSize, time)", new
                {
                    startTime,
                    endTime,
                    tag,
                    breakdownSize,
                    breakdownInterval
                })
                .ToList();
        }


        private object LoadOverTimeData(DateTime startTime, DateTime endTime, string tag, string breakdownSize, TimeSpan breakdownInterval)
        {
            return _context
                .Database.GetDbConnection()
                .Query<OverTimeQuery>(@"
with 
times as (
	select generate_series(date_trunc(@breakdownSize, @StartTime), date_trunc(@breakdownSize, @EndTime), @breakdownInterval) as time
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
time as Time,
SUM(time_total) over (partition by user_id order by time range between unbounded preceding and current row) as RunningTotal
from 
(
	select 
		siteUsers.id as user_id,
		siteUsers.display_name,
		siteUsers.is_moderator,
		date_trunc(@breakdownSize, user_actions.time) as time,
		SUM(case when user_actions.id is null then 0 else 1 end) as time_total
	from 
		siteUsers
	inner join user_actions 
		on user_actions.site_user_id = siteUsers.id
		and user_actions.tag = @Tag and user_actions.time > @StartTime and user_actions.time < @EndTime
	group by 
		siteUsers.id,
		siteUsers.display_name,
		siteUsers.is_moderator,
		date_trunc(@breakdownSize, user_actions.time)

    UNION ALL

    select 
	siteUsers.id as user_id,
	siteUsers.display_name,
	siteUsers.is_moderator,
	time,
    0 as time_total
    from siteUsers
    left join lateral (select time from times) t on true
) hourlyQuery;", new {
                    StartTime = startTime,
                    EndTime = endTime,
                    Tag = tag,
                    breakdownSize,
                    breakdownInterval
                })
                .GroupBy(g => new { g.DisplayName, g.IsModerator, g.UserId })
                .Select(g => new
                {
                    UserName = g.Key.DisplayName,
                    g.Key.IsModerator,
                    Times = g.OrderBy(gg => gg.Time).Select(gg => new
                    {
                        Date = gg.Time,
                        Total = gg.RunningTotal
                    })
                })
                .OrderByDescending(g => g.Times.Max(tt => tt.Total))
                .ToList();
        }

        private List<UserTotalsData> LoadUserTotalsData(DateTime startTime, DateTime endTime, DateTime burnStart, string tag)
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
and time > @burnStart
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
                    tag
                })
                .ToList();
        }

        private List<UserTotalsData> LoadUserGrandTotalsData(DateTime startTime, DateTime endTime, DateTime burnStart, string tag)
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
and time > @burnStart
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
                    tag
                })
                .ToList();
        }

        private class ZombieQuery
        {
            public int ZombieCount { get; set; }
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
            public DateTime Time { get; set; }
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