using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YouTrackSharp;
using YouTrackSharp.Issues;
using YouTrackSharp.TimeTracking;
using YouTrackSharp.Users;

namespace Werk.Services.YouTrack
{
    public class YouTrackConnection : IYouTrackConnection
    {
        private readonly IOptions<YouTrackOptions> _options;

        private readonly Lazy<BearerTokenConnection> _connection;

        private readonly Lazy<IIssuesService> _issuesService;

        private readonly Lazy<ITimeTrackingService> _timeTrackingService;

        private readonly Lazy<IUserService> _userService;

        public YouTrackConnection(IOptions<YouTrackOptions> options)
        {
            _options = options;

            _connection = new Lazy<BearerTokenConnection>(() =>
            {
                return new BearerTokenConnection(_options.Value.ServerUrl, _options.Value.BearerToken);
            });

            _issuesService = new Lazy<IIssuesService>(() => _connection.Value.CreateIssuesService());
            _timeTrackingService = new Lazy<ITimeTrackingService>(() => _connection.Value.CreateTimeTrackingService());
            _userService = new Lazy<IUserService>(() => new UserService(_connection.Value));
        }

        async Task<IEnumerable<Issue>> IYouTrackConnection.GetIssues(string filter)
        {
            return await _issuesService.Value.GetIssues(filter);
        }

        async Task<User> IYouTrackConnection.GetCurrentUser()
        {
            return await _userService.Value.GetCurrentUserInfo();
        }

        async Task<IEnumerable<WorkItem>> IYouTrackConnection.GetWorkItemsForIssue(string issueId)
        {
            return await _timeTrackingService.Value.GetWorkItemsForIssue(issueId);
        }
    }
}
