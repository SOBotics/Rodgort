using System.Collections.Generic;
using Newtonsoft.Json;

namespace StackExchangeApi.Responses
{
    public class BaseAnswer
    {
        public bool? Accepted { get; set; }

        [JsonProperty("answer_id")]
        public int? AnswerId { get; set; }

        [JsonProperty("awarded_bounty_amount")]
        public int? AwardedBountyAmount { get; set; }

        [JsonProperty("awarded_bounty_users")]
        public int? AwardedBountyUsers { get; set; }

        public string Body { get; set; }

        [JsonProperty("body_markdown")]
        public string BodyMarkdown { get; set; }

        [JsonProperty("can_flag")]
        public bool? CanFlag { get; set; }

        [JsonProperty("comment_count")]
        public int? CommentCount { get; set; }

        public string Comments { get; set; }

        [JsonProperty("community_owned_date")]
        public int? CommunityOwnedDate { get; set; }

        [JsonProperty("creation_date")]
        public int? CreationDate { get; set; }

        [JsonProperty("down_vote_count")]
        public int? DownvoteCount { get; set; }

        public bool? Downvoted { get; set; }

        [JsonProperty("is_accepted")]
        public bool? IsAccepted { get; set; }

        [JsonProperty("last_activity_date")]
        public int? LastActivityDate { get; set; }

        [JsonProperty("last_edit_date")]
        public int? LastEditDate { get; set; }

        [JsonProperty("last_editor")]
        public object LastEditor { get; set; }

        public string Link { get; set; }

        [JsonProperty("locked_date")]
        public int? LockedDate { get; set; }

        public object Owner { get; set; }

        [JsonProperty("question_id")]
        public int? QuestionId { get; set; }

        public int? Score { get; set; }

        [JsonProperty("share_link")]
        public string ShareLink { get; set; }

        public List<string> Tags { get; set; }

        public string Title { get; set; }

        [JsonProperty("up_vote_count")]
        public int? UpvoteCount { get; set; }

        public bool? Upvoted { get; set; }
    }
}
