﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(RodgortContext context, DateService dateService, ILogger<StatisticsController> logger)
        {
            _context = context;
            _dateService = dateService;
            _logger = logger;
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

            var numZombies = _context.ZombieTagsView.Where(z => z.Tag.NumberOfQuestions > 0 & z.Tag.SynonymOf == null).Select(z => z.TagName).Distinct().Count();
            var numUsers = _context.UserStatisticsView.Count();

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
            return _context
                .Database.GetDbConnection()
                .Query(@"
select 
	innerQuery.id,
	innerQuery.title,
	string_agg(distinct innerQuery.tag_name, ',') as tags,
	innerQuery.burn_started as ""burnStarted"",
	innerQuery.burn_ended as ""burnEnded"",
	sum(innerQuery.action_count) as ""numActions""
from (
	select
		meta_questions.id,
		meta_questions.title,
		meta_questions.burn_started,
		meta_questions.burn_ended,
		user_actions.site_user_id,
		meta_question_tags.tag_name,
		count(distinct user_actions.post_id) as action_count
	from meta_questions
	inner join meta_question_tags on meta_question_tags.meta_question_id = meta_questions.id
	left join user_actions 
		on user_actions.tag = meta_question_tags.tag_name 
		and time > meta_questions.burn_started 
		and (time < meta_questions.burn_ended or meta_questions.burn_ended is null)
	where meta_question_tags.tracking_status_id = 2
	and meta_questions.burn_started is not null
	group by meta_questions.id,
		meta_questions.title,
		meta_questions.burn_started,
		meta_questions.burn_ended,
		user_actions.site_user_id,
		meta_question_tags.tag_name
) innerQuery
group by 
	innerQuery.id,
	innerQuery.title,
	innerQuery.burn_started,
	innerQuery.burn_ended
order by innerQuery.burn_started desc
")
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

        public class UpdateSvgRequest
        {
            public int MetaQuestionId { get; set; }
            public string Svg { get; set; }
        }

        [HttpPost("UpdateSvg")]
        [Authorize]
        public void UpdateSvg([FromBody] UpdateSvgRequest request)
        {
            var metaQuestion = _context.MetaQuestions.FirstOrDefault(mq => mq.Id == request.MetaQuestionId);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (metaQuestion != null)
            {
                metaQuestion.ProgressSvg = request.Svg;
                _context.SaveChanges();
                _logger.LogInformation($"User {userName} ({User.UserId()}) uploaded svg for {request.MetaQuestionId}");
            }
            else
            {
                _logger.LogInformation($"User {userName} ({User.UserId()}) attempted to upload svg for {request.MetaQuestionId}");
            }
        }

        [HttpGet("{metaQuestionId}/Progress.svg")]
        public FileResult Svg(int metaQuestionId)
        {
            var metaQuestion = _context.MetaQuestions.FirstOrDefault(mq => mq.Id == metaQuestionId);
            if (metaQuestion != null)
                return File(Encoding.UTF8.GetBytes(metaQuestion.ProgressSvg), "image/svg+xml");

            return File(new byte[0], "image/svg+xml");
        }

        private object GenerateBurnsData(IQueryable<DbMetaQuestion> query)
        {
            var isRoomOwner = User.HasRole(DbRole.TRIAGER);
            var now = _dateService.UtcNow;
            var monthAgo = now.AddMonths(-1);
            var inAnHour = now.AddHours(1);
            var timeAfterBurn = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30));

            var burnsData = query
                .Select(mq => new
                {
                    mq.FeaturedStarted,
                    mq.FeaturedEnded,
                    mq.BurnStarted,
                    mq.BurnEnded,
                    StartTime = mq.FeaturedStarted ?? mq.FeaturedEnded ?? mq.BurnStarted ?? mq.BurnEnded ?? monthAgo,
                    EndTime = mq.FeaturedEnded.HasValue && !mq.BurnStarted.HasValue
                                ? mq.FeaturedEnded.Value.Add(timeAfterBurn)
                                : mq.BurnStarted.HasValue && mq.BurnEnded.HasValue
                                    ? mq.BurnEnded.Value.Add(timeAfterBurn)
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

                            UserTotals = LoadUserTotalsData(isRoomOwner ? b.StartTime : (b.BurnStarted ?? b.BurnEnded ?? inAnHour), b.EndTime, bt.Tag),

                            UserGrandTotals = LoadUserGrandTotalsData(isRoomOwner ? b.StartTime : (b.BurnStarted ?? b.BurnEnded ?? inAnHour), b.EndTime, bt.Tag)
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

        private List<UserTotalsData> LoadUserTotalsData(DateTime startTime, DateTime endTime, string tag)
        {
            return _context
                .Database.GetDbConnection()
                .Query<UserTotalsData>(@"
select 
	site_users.id as UserId,
	site_users.display_name as UserName,
	site_users.is_moderator as IsModerator,
    site_users.reputation as Reputation,
	user_action_types.name as Type,
	COUNT(distinct user_actions.post_id) as Total
from user_actions
inner join site_users on user_actions.site_user_id = site_users.id
inner join user_action_types on user_actions.user_action_type_id = user_action_types.id
where (tag = @tag and time > @startTime and time < @endTime)
and site_users.id > 0
group by 
	site_users.id,
	site_users.display_name,
	site_users.is_moderator,
    site_users.reputation,
	user_action_types.name
order by COUNT(distinct user_actions.post_id) desc", new
                {
                    startTime,
                    endTime,
                    tag
                })
                .ToList();
        }

        private List<UserTotalsData> LoadUserGrandTotalsData(DateTime startTime, DateTime endTime, string tag)
        {
            return _context
                .Database.GetDbConnection()
                .Query<UserTotalsData>(@"
select 
	site_users.id as UserId,
	site_users.display_name as UserName,
	site_users.is_moderator as IsModerator,
    site_users.reputation as Reputation,
	COUNT(distinct user_actions.post_id) as Total
from user_actions
inner join site_users on user_actions.site_user_id = site_users.id
where (tag = @tag and time > @startTime and time < @endTime)
and site_users.id > 0
group by 
	site_users.id,
	site_users.display_name,
	site_users.is_moderator,
    site_users.reputation
order by COUNT(distinct user_actions.post_id) desc", new
                {
                    startTime,
                    endTime,
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
            public int Reputation { get; set; }
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