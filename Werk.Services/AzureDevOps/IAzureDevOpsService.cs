using System.Linq;
using System.Threading.Tasks;

namespace Werk.Services.AzureDevOps
{
    public interface IAzureDevOpsService : IWerkService
    {
        Task<IOrderedEnumerable<PullRequest>> FetchAllPullRequests();
    }
}
