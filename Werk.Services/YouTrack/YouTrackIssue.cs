using System;
using Werk.Utility;
using YouTrackSharp.Issues;

namespace Werk.Services.YouTrack
{
    public class YouTrackIssue
    {
        public string Id { get; init; }

        public string Summary { get; init; }

        public string Description { get; init; }

        public string Link { get; init; }

        public YouTrackIssue()
        {

        }

        public YouTrackIssue(Issue issue, Uri serverUri)
        {
            Id = issue.Id;
            Summary = issue.Summary;
            Description = issue.Description;
            Link = $"{serverUri.WithEndingSlash().AbsoluteUri}issue/{issue.Id}";
        }
    }
}
