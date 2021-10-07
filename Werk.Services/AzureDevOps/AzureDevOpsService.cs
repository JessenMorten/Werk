using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Werk.Services.Cache;
using Werk.Utility;

namespace Werk.Services.AzureDevOps
{
    public class AzureDevOpsService : IAzureDevOpsService
    {
        private readonly IOptions<AzureDevOpsOptions> _options;

        private readonly ICacheService _cacheService;

        public AzureDevOpsService(IOptions<AzureDevOpsOptions> options, ICacheService cacheService)
        {
            _options = options;
            _cacheService = cacheService;
        }

        public async Task<WerkServiceStatus> GetStatus()
        {
            var key = $"{nameof(AzureDevOpsService)}.{nameof(WerkServiceStatus)}";
            var maxAge = TimeSpan.FromHours(5);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var isDisabled =
                    string.IsNullOrWhiteSpace(_options.Value.ServerUrl) ||
                    string.IsNullOrWhiteSpace(_options.Value.PersonalAccessToken) ||
                    string.IsNullOrWhiteSpace(_options.Value.CollectionName) ||
                    string.IsNullOrWhiteSpace(_options.Value.UniqueName);

                if (isDisabled)
                {
                    return WerkServiceStatus.Disabled;
                }

                try
                {
                    var me = await FetchMe();
                    return me is null ? WerkServiceStatus.NotReady : WerkServiceStatus.Ready;
                }
                catch
                {
                    return WerkServiceStatus.NotReady;
                }
            });
        }
        public async Task<IEnumerable<Project>> FetchAllProjects()
        {
            var key = $"{nameof(AzureDevOpsService)}.AllProjects";
            var maxAge = TimeSpan.FromHours(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = "_apis/projects";
                var response = await GetFromJson<ListResponse<Project>>(requestUri);
                return response.Value ?? Enumerable.Empty<Project>();
            });
        }

        public async Task<IEnumerable<Team>> FetchAllTeams()
        {
            var key = $"{nameof(AzureDevOpsService)}.AllTeams";
            var maxAge = TimeSpan.FromHours(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = "_apis/teams";
                var response = await GetFromJson<ListResponse<Team>>(requestUri);
                return response.Value ?? Enumerable.Empty<Team>();
            });
        }

        public async Task<IEnumerable<Member>> FetchAllMembers()
        {
            var key = $"{nameof(AzureDevOpsService)}.AllMembers";
            var maxAge = TimeSpan.FromHours(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var teams = await FetchAllTeams();
                var memberTasks = teams.Select(FetchMembers).ToList();
                await Task.WhenAll(memberTasks);
                return memberTasks.SelectMany(t => t.Result);
            });
        }

        public async Task<Member> FetchMe()
        {
            var key = $"{nameof(AzureDevOpsService)}.Me";
            var maxAge = TimeSpan.FromHours(5);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var members = await FetchAllMembers();
                return members.FirstOrDefault(m => m.Identity.UniqueName == _options.Value.UniqueName);
            });
        }

        public async Task<IEnumerable<Member>> FetchMembers(Team team)
        {
            var key = $"{nameof(AzureDevOpsService)}.MembersByTeam.{team.Id}";
            var maxAge = TimeSpan.FromHours(5);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = $"_apis/projects/{team.ProjectId}/teams/{team.Id}/members";
                var response = await GetFromJson<ListResponse<Member>>(requestUri);
                return response.Value ?? Enumerable.Empty<Member>();
            });
        }

        public async Task<IEnumerable<Repository>> FetchAllRepositories()
        {
            var key = $"{nameof(AzureDevOpsService)}.AllRepositories";
            var maxAge = TimeSpan.FromHours(3);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var projects = await FetchAllProjects();
                var repositoryTasks = projects.Select(FetchRepositories).ToList();
                await Task.WhenAll(repositoryTasks);
                return repositoryTasks.SelectMany(t => t.Result);
            });
        }

        public async Task<IEnumerable<Repository>> FetchRepositories(Project project)
        {
            var key = $"{nameof(AzureDevOpsService)}.RepositoriesByProject.{project.Name}";
            var maxAge = TimeSpan.FromHours(3);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = $"{project.Name}/_apis/git/repositories?api-version=6.0";
                var response = await GetFromJson<ListResponse<Repository>>(requestUri);
                return response.Value ?? Enumerable.Empty<Repository>();
            });
        }

        public async Task<IEnumerable<PullRequest>> FetchAllPullRequests()
        {
            var key = $"{nameof(AzureDevOpsService)}.AllPullRequests";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var repositories = await FetchAllRepositories();
                var pullRequestTasks = repositories.Select(FetchPullRequests).ToList();
                await Task.WhenAll(pullRequestTasks);
                return pullRequestTasks.SelectMany(t => t.Result);
            });
        }

        public async Task<IEnumerable<PullRequest>> FetchPullRequests(Repository repository)
        {
            var key = $"{nameof(AzureDevOpsService)}.PullRequestsByRepository.{repository.Name}";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = $"{repository.Project.Name}/_apis/git/repositories/{repository.Name}/pullrequests?searchCriteria.status=active";
                var briefPullRequests = await GetFromJson<ListResponse<BriefPullRequest>>(requestUri);

                IEnumerable<PullRequest> pullRequests = null;

                if (briefPullRequests.Value is not null && briefPullRequests.Value.Any())
                {
                    var pullRequestTasks = briefPullRequests
                    .Value
                    .Select(pr => GetFromJson<PullRequest>(pr.Url))
                    .ToList();

                    await Task.WhenAll(pullRequestTasks);

                    pullRequests = pullRequestTasks.Select(t => t.Result);
                }

                return pullRequests ?? Enumerable.Empty<PullRequest>();
            });
        }

        public async Task<IEnumerable<PullRequest>> FetchPullRequests(Member member)
        {
            var key = $"{nameof(AzureDevOpsService)}.PullRequestsByMember.{member.Identity.UniqueName}";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var pullRequests = await FetchAllPullRequests();
                return pullRequests.Where(p => p.createdBy.uniqueName == member.Identity.UniqueName);
            });
        }

        public async Task<IEnumerable<PullRequest>> FetchMyPullRequests()
        {
            var key = $"{nameof(AzureDevOpsService)}.MyPullRequests";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var me = await FetchMe();
                return await FetchPullRequests(me);
            });
        }

        private async Task<T> GetFromJson<T>(string requestUri)
        {
            // Create http client
            var baseAddress = _options.Value.ServerUrl + "/" + _options.Value.CollectionName;
            using var httpClient = new HttpClient
            {
                BaseAddress = UriUtility.Create(baseAddress).WithEndingSlash()
            };

            // Clear headers
            httpClient.DefaultRequestHeaders.Clear();

            // Set accept header
            var acceptHeader = new MediaTypeWithQualityHeaderValue("application/json");
            httpClient.DefaultRequestHeaders.Accept.Add(acceptHeader);

            // Set authorization header
            var authParameter = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_options.Value.PersonalAccessToken}"));
            var authHeader = new AuthenticationHeaderValue("Basic", authParameter);
            httpClient.DefaultRequestHeaders.Authorization = authHeader;

            // Send request
            return await httpClient.GetFromJsonAsync<T>(requestUri);
        }

        private class ListResponse<T>
        {
            [JsonPropertyName("count")]
            public int Count { get; init; }

            [JsonPropertyName("value")]
            public IEnumerable<T> Value { get; init; }
        }
    }
}
