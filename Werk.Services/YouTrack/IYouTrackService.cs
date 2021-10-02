using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Werk.Services.YouTrack
{
    public interface IYouTrackService : IWerkService
    {
        Task<YouTrackUser> FetchMe();

        /// <summary>
        /// Fetches all <see cref="YouTrackWorkItem"/>s authored by the current user on the specified date.
        /// </summary>
        Task<IEnumerable<YouTrackWorkItem>> FetchMyWorkItems(DateTime workDate);

        /// <summary>
        /// Fetches all <see cref="YouTrackIssue"/>s with work items authored by the current user,
        /// that was resolved on the specified date.
        /// </summary>
        Task<IEnumerable<YouTrackIssue>> FetchMyResolvedIssues(DateTime resolvedDate);
    }
}
