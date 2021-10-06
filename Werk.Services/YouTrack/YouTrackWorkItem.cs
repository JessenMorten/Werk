using System;
using System.Text.Json.Serialization;
using YouTrackSharp.TimeTracking;

namespace Werk.Services.YouTrack
{
    public class YouTrackWorkItem
    {
        public string IssueId { get; init; }

        public string IssueSummary { get; init; }

        public string IssueLink { get; init; }

        public string IssueDescription { get; init; }

        public string Description { get; init; }

        public int DurationInSeconds { get; set; }

        [JsonIgnore]
        public TimeSpan Duration => TimeSpan.FromSeconds(DurationInSeconds);

        public DateTime WorkDate { get; init; }

        public YouTrackWorkItem()
        {

        }

        public YouTrackWorkItem(YouTrackIssue issue, WorkItem workItem)
        {
            IssueId = issue.Id;
            IssueSummary = issue.Summary;
            IssueLink = issue.Link;
            IssueDescription = issue.Description;
            DurationInSeconds = (int)workItem.Duration.TotalSeconds;
            WorkDate = workItem.Date.Value.Date;
            Description = workItem.Description;
        }
    }
}
