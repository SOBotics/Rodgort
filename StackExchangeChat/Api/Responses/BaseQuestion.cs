using System.Collections.Generic;
using Newtonsoft.Json;

namespace StackExchangeChat.Api.Responses
{
    public class BaseQuestion
    {
        [JsonProperty("accepted_answer_id")]
        public int? AcceptedAnswerId { get; set; }

        [JsonProperty("answer_count")]
        public int? AnswerCount { get; set; }

        public object Answers { get; set; }
        public string Body { get; set; }

        [JsonProperty("body_markdown")]
        public string BodyMarkdown { get; set; }

        [JsonProperty("bounty_amount")]
        public int? BountyAmount { get; set; }

        [JsonProperty("bounty_closes_date")]
        public int? BountyClosesDate { get; set; }

        [JsonProperty("bounty_user")]
        public object BountyUser { get; set; }

        [JsonProperty("can_close")]
        public bool? CanClose { get; set; }

        [JsonProperty("can_flag")]
        public bool? CanFlag { get; set; }

        [JsonProperty("close_vote_count")]
        public bool? CloseVoteCount { get; set; }

        [JsonProperty("closed_date")]
        public int? ClosedDate { get; set; }

        [JsonProperty("closed_reason")]
        public string ClosedReason { get; set; }

        [JsonProperty("comment_count")]
        public int? CommentCount { get; set; }

        public object Comments { get; set; }

        [JsonProperty("community_owned_date")]
        public int? CommunityOwnedDate { get; set; }

        [JsonProperty("creation_date")]
        public int? CreationDate { get; set; }

        [JsonProperty("delete_vote_count")]
        public int? DeleteVoteCount { get; set; }

        [JsonProperty("down_vote_count")]
        public int? DownVoteCount { get; set; }

        public bool? Downvoted { get; set; }

        [JsonProperty("favorite_count")]
        public int? FavouriteCount { get; set; }

        [JsonProperty("favorited")]
        public bool? Favourited { get; set; }

        [JsonProperty("is_answered")]
        public bool? IsAnswered { get; set; }

        [JsonProperty("last_activity_date")]
        public int? LastActivityDate { get; set; }

        [JsonProperty("last_edit_date")]
        public int? LastEditDate { get; set; }

        [JsonProperty("last_editor")]
        public object LastEditor { get; set; }

        public string Link { get; set; }

        [JsonProperty("locked_date")]
        public int? LockedDate { get; set; }

        [JsonProperty("migrated_from")]
        public object MigratedFrom { get; set; }

        [JsonProperty("migrated_to")]
        public object MigratedTo { get; set; }

        public object Notice { get; set; }
        public object Owner { get; set; }

        [JsonProperty("protected_date")]
        public int? ProtectedDate { get; set; }

        [JsonProperty("question_id")]
        public int? QuestionId { get; set; }

        [JsonProperty("reopen_vote_count")]
        public int? ReopenVoteCount { get; set; }

        public int? Score { get; set; }

        [JsonProperty("share_link")]
        public string ShareLink { get; set; }

        public List<string> Tags { get; set; }

        public string Title { get; set; }

        [JsonProperty("up_vote_count")]
        public int? UpvoteCount { get; set; }

        public bool? Upvoted { get; set; }

        [JsonProperty("view_count")]
        public int? ViewCount { get; set; }
    }
}
