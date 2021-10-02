using System;
using YouTrackSharp.Issues;
using YouTrackSharp.TimeTracking;

namespace Werk.Services.YouTrack
{
    public class YouTrackWorkItem
    {
        public string IssueId { get; }

        public string IssueSummary { get; }

        public string IssueDescription { get; }

        public string Description { get; }

        public TimeSpan Duration { get; }

        public DateTime WorkDate { get; }

        internal YouTrackWorkItem(YouTrackIssue issue, WorkItem workItem)
        {
            IssueId = issue.Id;
            IssueSummary = issue.Summary;
            IssueDescription = issue.Description;
            Duration = workItem.Duration;
            WorkDate = workItem.Date.Value.Date;
            Description = workItem.Description;
        }
    }
}
