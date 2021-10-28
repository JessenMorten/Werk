using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Werk.Services.Cache;
using Werk.Utility;

namespace Werk.Services.YouTrack
{
    public class YouTrackService : IYouTrackService
    {
        private readonly Lazy<Uri> _serverUri;

        private readonly IOptions<YouTrackOptions> _options;

        private readonly IYouTrackConnection _connection;

        private readonly ICacheService _cacheService;

        public YouTrackService(IOptions<YouTrackOptions> options, IYouTrackConnection connection, ICacheService cacheService)
        {
            _options = options;
            _connection = connection;
            _cacheService = cacheService;
            _serverUri = new Lazy<Uri>(() => UriUtility.Create(_options.Value.ServerUrl));
        }

        public async Task<WerkServiceStatus> GetStatus()
        {
            var key = $"{nameof(YouTrackService)}.{nameof(WerkServiceStatus)}";
            var maxAge = TimeSpan.FromHours(5);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var isDisabled = string.IsNullOrWhiteSpace(_options.Value.ServerUrl) || string.IsNullOrWhiteSpace(_options.Value.BearerToken);

                if (isDisabled)
                {
                    return WerkServiceStatus.Disabled;
                }

                try
                {
                    _ = await FetchMyResolvedIssues(DateTime.Now);
                    return WerkServiceStatus.Ready;
                }
                catch
                {
                    return WerkServiceStatus.NotReady;
                }
            });
        }

        public async Task<YouTrackUser> FetchMe()
        {
            var key = $"{nameof(YouTrackService)}.Me";
            var maxAge = TimeSpan.FromHours(2);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var user = await _connection.GetCurrentUser();
                return new YouTrackUser(user, _serverUri.Value);
            });
        }

        public async Task<HourReport> FetchHourReport()
        {
            // Get week day
            static int GetWeekDayNumber(DayOfWeek dayOfWeek) => dayOfWeek switch
            {
                DayOfWeek.Monday => 1,
                DayOfWeek.Tuesday => 2,
                DayOfWeek.Wednesday => 3,
                DayOfWeek.Thursday => 4,
                DayOfWeek.Friday => 5,
                DayOfWeek.Saturday => 6,
                DayOfWeek.Sunday => 7,
                _ => throw new InvalidOperationException($"Unknown {nameof(DayOfWeek)}: '{dayOfWeek}'")
            };

            var key = $"{nameof(YouTrackService)}.{nameof(HourReport)}";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                // Hours worked today
                var now = DateTime.Now;
                var hoursWorkedToday = (await FetchMyWorkItems(now)).Sum(w => w.Duration.TotalHours);

                // Hours worked previous days this week
                var previousDaysTasks = Enumerable
                    .Range(0, GetWeekDayNumber(now.DayOfWeek) - 1)
                    .Select(i => FetchMyWorkItems(now.AddDays((i + 1) * -1)))
                    .ToList();

                await Task.WhenAll(previousDaysTasks);
                var hoursWorkedPreviousDays = previousDaysTasks
                    .SelectMany(t => t.Result)
                    .Sum(w => w.Duration.TotalHours);

                // Total hours this week
                var totalHoursThisWeek = hoursWorkedToday + hoursWorkedPreviousDays;

                // Remaining hours this week
                var remainingHoursThisWeek = _options.Value.WeeklyWorkHours - totalHoursThisWeek;

                // Remaining hours today
                var remainingWorkDays = Math.Max(0, 1 + GetWeekDayNumber(DayOfWeek.Friday) - GetWeekDayNumber(now.DayOfWeek));
                var remainingHoursToday = 0.0;
                if (remainingHoursThisWeek > 0 && remainingWorkDays > 0)
                {
                    var hoursPerDay = (remainingHoursThisWeek + hoursWorkedToday) / remainingWorkDays;
                    remainingHoursToday = hoursPerDay - hoursWorkedToday;
                }

                // Return report
                return new HourReport
                {
                    WeeklyWorkHours = _options.Value.WeeklyWorkHours,
                    HoursWorkedThisWeek = totalHoursThisWeek,
                    HoursWorkedToday = hoursWorkedToday,
                    RemainingWorkHoursWeek = remainingHoursThisWeek,
                    RemainingWorkHoursToday = remainingHoursToday
                };
            });
        }

        public async Task<IEnumerable<YouTrackIssue>> FetchMyResolvedIssues(DateTime resolvedDate)
        {
            var key = $"{nameof(YouTrackService)}.ResolvedIssues.{resolvedDate:yyyy-MM-dd}";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var filter = $"resolved date: {resolvedDate:yyyy-MM-dd} work author: me";
                var issues = await _connection.GetIssues(filter);
                return issues.Select(issue => new YouTrackIssue(issue, _serverUri.Value));
            });
        }

        public async Task<IEnumerable<YouTrackIssue>> FetchMyUnresolvedIssues()
        {
            var key = $"{nameof(YouTrackService)}.MyUnresolvedIssues";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var filter = $"assignee: me -resolved";
                var issues = await _connection.GetIssues(filter);
                return issues.Select(issue => new YouTrackIssue(issue, _serverUri.Value));
            });
        }

        public async Task<IEnumerable<YouTrackWorkItem>> FetchMyWorkItems(DateTime workDate)
        {
            var key = $"{nameof(YouTrackService)}.WorkItems.{workDate:yyyy-MM-dd}";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                // Get issues
                var filter = $"work date: {workDate:yyyy-MM-dd} work author: me";
                var issues = await _connection.GetIssues(filter);

                // Get workitems
                var me = await _connection.GetCurrentUser();
                var workItemTasks = issues.Select(async issue =>
                {
                    var youTrackIssue = new YouTrackIssue(issue, _serverUri.Value);
                    var workItems = await _connection.GetWorkItemsForIssue(issue.Id);
                    return workItems
                        .Where(workItem => workItem.Author.Login == me.Login)
                        .Select(workItem => new YouTrackWorkItem(youTrackIssue, workItem));
                }).ToList();
                await Task.WhenAll(workItemTasks);

                // Get workitems where workdate matches
                return workItemTasks
                    .SelectMany(task => task.Result)
                    .Where(workItem => workItem.WorkDate == workDate.Date)
                    .ToList();
            });
        }

        public async Task<(DateTime date, IOrderedEnumerable<YouTrackWorkItem> workItems)> FetchLastWorkingDaysWorkItems(int maxDays = 7)
        {
            var workItems = Enumerable.Empty<YouTrackWorkItem>();
            var date = DateTime.UtcNow;

            for (int i = 1; i <= maxDays; i++)
            {
                date = date.AddDays(-i);
                workItems = await FetchMyWorkItems(date);

                if (workItems.Any())
                {
                    break;
                }
            }

            return (date, workItems.OrderByDescending(w => w.Duration));
        }
    }
}
