using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Werk.Utility;

namespace Werk.Services.YouTrack
{
    public class YouTrackService : IYouTrackService
    {
        private readonly Lazy<Uri> _serverUri;

        private readonly IOptions<YouTrackOptions> _options;

        private readonly IYouTrackConnection _connection;

        private WerkServiceStatus _status;

        public YouTrackService(IOptions<YouTrackOptions> options, IYouTrackConnection connection)
        {
            _options = options;
            _connection = connection;
            _serverUri = new Lazy<Uri>(() => UriUtility.Create(_options.Value.ServerUrl));
            _status = WerkServiceStatus.Unknown;
        }

        public async Task<WerkServiceStatus> GetStatus()
        {
            if (string.IsNullOrWhiteSpace(_options.Value.ServerUrl) || string.IsNullOrWhiteSpace(_options.Value.BearerToken))
            {
                _status = WerkServiceStatus.Disabled;
            }

            if (_status == WerkServiceStatus.Unknown)
            {
                try
                {
                    _ = await FetchMyResolvedIssues(DateTime.UtcNow);
                    _status = WerkServiceStatus.Ready;
                }
                catch
                {
                    _status = WerkServiceStatus.NotReady;
                }
            }

            return _status;
        }

        public async Task<YouTrackUser> FetchMe()
        {
            return new YouTrackUser(await _connection.GetCurrentUser());
        }

        public async Task<IEnumerable<YouTrackIssue>> FetchMyResolvedIssues(DateTime resolvedDate)
        {
            var filter = $"resolved date: {resolvedDate:yyyy-MM-dd} work author: me";
            var issues = await _connection.GetIssues(filter);
            return issues.Select(issue => new YouTrackIssue(issue, _serverUri.Value));
        }

        public async Task<IEnumerable<YouTrackWorkItem>> FetchMyWorkItems(DateTime workDate)
        {
            // Get issues
            var filter = $"work date: {workDate:yyyy-MM-dd} work author: me";
            var issues = await _connection.GetIssues(filter);

            // Get workitems
            var me = await _connection.GetCurrentUser();
            var workItemTasks = issues.Select(async issue =>
            {
                var youTrackIssue = new YouTrackIssue(issue, _serverUri.Value);
                var workItems = await _connection.GetWorkItemsForIssue(issue);
                return workItems
                    .Where(workItem => workItem.Author.Login == me.Login)
                    .Select(workItem => new YouTrackWorkItem(youTrackIssue, workItem));
            });
            await Task.WhenAll(workItemTasks.ToList());

            // Get workitems where workdate matches
            return workItemTasks
                .SelectMany(task => task.Result)
                .Where(workItem => workItem.WorkDate == workDate.Date)
                .ToList();
        }
    }
}
