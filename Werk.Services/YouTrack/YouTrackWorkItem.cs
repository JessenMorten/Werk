using System;
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

        public TimeSpan Duration { get; init; }

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
            Duration = workItem.Duration;
            WorkDate = workItem.Date.Value.Date;
            Description = workItem.Description;
        }
    }
}
