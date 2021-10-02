using System;
using Werk.Utility;
using YouTrackSharp.Issues;

namespace Werk.Services.YouTrack
{
    public class YouTrackIssue
    {
        public string Id { get; }

        public string Summary { get; }

        public string Description { get; }

        public string Link { get; }

        public YouTrackIssue(Issue issue, Uri serverUri)
        {
            Id = issue.Id;
            Summary = issue.Summary;
            Description = issue.Description;
            Link = $"{serverUri.WithEndingSlash().AbsoluteUri}issue/{issue.Id}";
        }
    }
}
