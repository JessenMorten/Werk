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
using Werk.Services.AzureDevOps.ResponseModels;

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

        public async Task<IOrderedEnumerable<PullRequest>> FetchAllPullRequests()
        {
            var key = $"{nameof(AzureDevOpsService)}.AllPullRequests";
            var maxAge = TimeSpan.FromMinutes(1);

            var pullRequests = await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var repositories = await FetchAllRepositories();
                var pullRequestTasks = repositories.Select(FetchPullRequests).ToList();
                await Task.WhenAll(pullRequestTasks);
                return pullRequestTasks.SelectMany(t => t.Result);
            });

            return pullRequests.OrderByDescending(p => p.CreationDate);
        }

        private async Task<IEnumerable<ProjectResponse>> FetchAllProjects()
        {
            var key = $"{nameof(AzureDevOpsService)}.AllProjects";
            var maxAge = TimeSpan.FromHours(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = "_apis/projects";
                var response = await GetFromJson<ListResponse<ProjectResponse>>(requestUri);
                return response.Value ?? Enumerable.Empty<ProjectResponse>();
            });
        }

        private async Task<IEnumerable<TeamResponse>> FetchAllTeams()
        {
            var key = $"{nameof(AzureDevOpsService)}.AllTeams";
            var maxAge = TimeSpan.FromHours(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = "_apis/teams";
                var response = await GetFromJson<ListResponse<TeamResponse>>(requestUri);
                return response.Value ?? Enumerable.Empty<TeamResponse>();
            });
        }

        private async Task<IEnumerable<MemberResponse>> FetchAllMembers()
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

        private async Task<MemberResponse> FetchMe()
        {
            var key = $"{nameof(AzureDevOpsService)}.Me";
            var maxAge = TimeSpan.FromHours(5);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var members = await FetchAllMembers();
                return members.FirstOrDefault(m => m.Identity.UniqueName == _options.Value.UniqueName);
            });
        }

        private async Task<IEnumerable<MemberResponse>> FetchMembers(TeamResponse team)
        {
            var key = $"{nameof(AzureDevOpsService)}.MembersByTeam.{team.Id}";
            var maxAge = TimeSpan.FromHours(5);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = $"_apis/projects/{team.ProjectId}/teams/{team.Id}/members";
                var response = await GetFromJson<ListResponse<MemberResponse>>(requestUri);
                return response.Value ?? Enumerable.Empty<MemberResponse>();
            });
        }

        private async Task<IEnumerable<RepositoryResponse>> FetchAllRepositories()
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

        private async Task<IEnumerable<RepositoryResponse>> FetchRepositories(ProjectResponse project)
        {
            var key = $"{nameof(AzureDevOpsService)}.RepositoriesByProject.{project.Name}";
            var maxAge = TimeSpan.FromHours(3);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = $"{project.Name}/_apis/git/repositories?api-version=6.0";
                var response = await GetFromJson<ListResponse<RepositoryResponse>>(requestUri);
                return response.Value ?? Enumerable.Empty<RepositoryResponse>();
            });
        }

        private async Task<IEnumerable<PullRequest>> FetchPullRequests(RepositoryResponse repository)
        {
            var key = $"{nameof(AzureDevOpsService)}.PullRequestsByRepository.{repository.Name}";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var requestUri = $"{repository.Project.Name}/_apis/git/repositories/{repository.Name}/pullrequests?searchCriteria.status=active";
                var briefPullRequests = await GetFromJson<ListResponse<BriefPullRequestResponse>>(requestUri);

                IEnumerable<PullRequest> pullRequests = null;

                if (briefPullRequests.Value is not null && briefPullRequests.Value.Any())
                {
                    var pullRequestTasks = briefPullRequests
                        .Value
                        .Select(pr => GetFromJson<PullRequestResponse>(pr.Url))
                        .ToList();

                    await Task.WhenAll(pullRequestTasks);

                    pullRequests = pullRequestTasks.Select(t => new PullRequest(t.Result));
                }

                return pullRequests ?? Enumerable.Empty<PullRequest>();
            });
        }

        private async Task<IEnumerable<PullRequest>> FetchPullRequests(MemberResponse member)
        {
            var key = $"{nameof(AzureDevOpsService)}.PullRequestsByMember.{member.Identity.UniqueName}";
            var maxAge = TimeSpan.FromMinutes(1);

            return await _cacheService.GetOrSet(key, maxAge, async () =>
            {
                var pullRequests = await FetchAllPullRequests();
                return pullRequests.Where(p => p.Creator.UniqueName == member.Identity.UniqueName);
            });
        }

        private async Task<IEnumerable<PullRequest>> FetchMyPullRequests()
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
