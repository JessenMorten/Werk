using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Werk.Utility;

namespace Werk.Services.AzureDevOps
{
    public class AzureDevOpsService : IAzureDevOpsService
    {
        private readonly Lazy<Uri> _serverUri;

        private readonly IOptions<AzureDevOpsOptions> _options;

        private readonly IDistributedCache _cache;

        private WerkServiceStatus _status;

        public AzureDevOpsService(IOptions<AzureDevOpsOptions> options, IDistributedCache cache)
        {
            _options = options;
            _cache = cache;
            _serverUri = new Lazy<Uri>(() => UriUtility.Create(_options.Value.ServerUrl));
        }

        public async Task<WerkServiceStatus> GetStatus()
        {
            if (_status == WerkServiceStatus.Unknown)
            {
                var isConfigured =
                    !string.IsNullOrWhiteSpace(_options.Value.ServerUrl) &&
                    !string.IsNullOrWhiteSpace(_options.Value.PersonalAccessToken) &&
                    !string.IsNullOrWhiteSpace(_options.Value.CollectionName) &&
                    !string.IsNullOrWhiteSpace(_options.Value.UniqueName);

                if (isConfigured)
                {
                    try
                    {
                        var me = await FetchMe();
                        _status = me is null ? WerkServiceStatus.NotReady : WerkServiceStatus.Ready;
                    }
                    catch
                    {
                        _status = WerkServiceStatus.NotReady;
                    }
                }
                else
                {
                    _status = WerkServiceStatus.Disabled;
                } 
            }

            return _status;
        }
        public async Task<IEnumerable<Project>> FetchAllProjects()
        {
            var requestUri = CreatePath() + "_apis/projects";
            var response = await Get<ListResponse<Project>>(requestUri, TimeSpan.FromMinutes(5));
            return response.Value ?? Enumerable.Empty<Project>();
        }

        public async Task<IEnumerable<Team>> FetchAllTeams()
        {
            var requestUri = CreatePath() + "_apis/teams";
            var response = await Get<ListResponse<Team>>(requestUri, TimeSpan.FromMinutes(5));
            return response.Value ?? Enumerable.Empty<Team>();
        }

        public async Task<IEnumerable<Member>> FetchAllMembers()
        {
            var teams = await FetchAllTeams();
            var memberTasks = teams.Select(FetchMembers).ToList();
            await Task.WhenAll(memberTasks);
            return memberTasks.SelectMany(t => t.Result);
        }

        public async Task<Member> FetchMe()
        {
            var members = await FetchAllMembers();
            return members.FirstOrDefault(m => m.Identity.UniqueName == _options.Value.UniqueName);
        }

        public async Task<IEnumerable<Member>> FetchMembers(Team team)
        {
            var requestUri = CreatePath() + $"_apis/projects/{team.ProjectId}/teams/{team.Id}/members";
            var response = await Get<ListResponse<Member>>(requestUri, TimeSpan.FromMinutes(5));
            return response.Value ?? Enumerable.Empty<Member>();
        }

        public async Task<IEnumerable<Repository>> FetchAllRepositories()
        {
            var projects = await FetchAllProjects();
            var repositoryTasks = projects.Select(FetchRepositories).ToList();
            await Task.WhenAll(repositoryTasks);
            return repositoryTasks.SelectMany(t => t.Result);
        }

        public async Task<IEnumerable<Repository>> FetchRepositories(Project project)
        {
            var requestUri = CreatePath(project.Name) + "_apis/git/repositories?api-version=6.0";
            var response = await Get<ListResponse<Repository>>(requestUri, TimeSpan.FromMinutes(5));
            return response.Value ?? Enumerable.Empty<Repository>();
        }

        public async Task<IEnumerable<PullRequest>> FetchAllPullRequests()
        {
            var repositories = await FetchAllRepositories();
            var pullRequestTasks = repositories.Select(FetchPullRequests).ToList();
            await Task.WhenAll(pullRequestTasks);
            return pullRequestTasks.SelectMany(t => t.Result);
        }

        public async Task<IEnumerable<PullRequest>> FetchPullRequests(Repository repository)
        {
            var requestUri = CreatePath(repository.Project.Name) + $"_apis/git/repositories/{repository.Name}/pullrequests?searchCriteria.status=active";
            var response = await Get<ListResponse<PullRequest>>(requestUri, TimeSpan.FromMinutes(1));
            return response.Value ?? Enumerable.Empty<PullRequest>();
        }

        public async Task<IEnumerable<PullRequest>> FetchPullRequests(Member member)
        {
            var pullRequests = await FetchAllPullRequests();
            return pullRequests.Where(p => p.CreatedBy.UniqueName == member.Identity.UniqueName);
        }

        public async Task<IEnumerable<PullRequest>> FetchMyPullRequests()
        {
            var me = await FetchMe();
            return await FetchPullRequests(me);
        }

        private HttpClient CreateHttpClient()
        {
            var acceptHeader = new MediaTypeWithQualityHeaderValue("application/json");

            var authParameter = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_options.Value.PersonalAccessToken}"));
            var authHeader = new AuthenticationHeaderValue("Basic", authParameter);

            var httpClient = new HttpClient();
            httpClient.BaseAddress = _serverUri.Value.WithEndingSlash();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(acceptHeader);
            httpClient.DefaultRequestHeaders.Authorization = authHeader;
            return httpClient;
        }

        private string CreatePath(string projectName = default)
        {
            if (projectName is null)
            {
                return $"{_options.Value.CollectionName}/";
            }
            else
            {
                return $"{_options.Value.CollectionName}/{projectName}/";
            }
        }

        private async Task<T> Get<T>(string requestUri, TimeSpan expires)
        {
            var jsonFromCache = await _cache.GetStringAsync(requestUri);
            var response = jsonFromCache is null ? default : JsonSerializer.Deserialize<T>(jsonFromCache);

            if (response is null)
            {
                using var httpClient = CreateHttpClient();
                response = await httpClient.GetFromJsonAsync<T>(requestUri);
                var json = JsonSerializer.Serialize(response);
                await _cache.SetStringAsync(requestUri, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expires
                });
            }

            return response;
        }

        private class ListResponse<T>
        {
            [JsonPropertyName("count")]
            public int Count { get; set; }

            [JsonPropertyName("value")]
            public IEnumerable<T> Value { get; set; }
        }
    }
}
