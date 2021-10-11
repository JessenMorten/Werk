using System.Collections.Generic;
using System.Threading.Tasks;

namespace Werk.Services.News
{
    public interface INewsService : IWerkService
    {
        Task<IEnumerable<News>> FetchNews(int maxCount = 5);
    }
}
