using System.Collections.Generic;
using System.Threading.Tasks;

namespace Werk.Services.AzureDevOps
{
    public interface IAzureDevOpsService : IWerkService
    {
        Task<IEnumerable<Project>> FetchAllProjects();

        Task<IEnumerable<Team>> FetchAllTeams();

        Task<IEnumerable<Member>> FetchAllMembers();

        Task<IEnumerable<Member>> FetchMembers(Team team);

        Task<Member> FetchMe();

        Task<IEnumerable<Repository>> FetchAllRepositories();

        Task<IEnumerable<Repository>> FetchRepositories(Project project);

        Task<IEnumerable<PullRequest>> FetchAllPullRequests();

        Task<IEnumerable<PullRequest>> FetchMyPullRequests();

        Task<IEnumerable<PullRequest>> FetchPullRequests(Repository repository);

        Task<IEnumerable<PullRequest>> FetchPullRequests(Member member);
    }
}
