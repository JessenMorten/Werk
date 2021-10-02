using System.Collections.Generic;
using System.Threading.Tasks;
using YouTrackSharp.Issues;
using YouTrackSharp.TimeTracking;
using YouTrackSharp.Users;

namespace Werk.Services.YouTrack
{
    public interface IYouTrackConnection
    {
        internal Task<IEnumerable<Issue>> GetIssues(string filter);

        internal Task<User> GetCurrentUser();

        internal Task<IEnumerable<WorkItem>> GetWorkItemsForIssue(Issue issue);

        internal Task<IEnumerable<WorkItem>> GetWorkItemsForIssue(string issueId);
    }
}
