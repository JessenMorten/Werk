using System;
using System.Threading.Tasks;

namespace Werk.Services.Cache
{
    public interface ICacheService
    {
        Task<T> GetOrSet<T>(string key, Func<Task<T>> func);

        Task<T> GetOrSet<T>(string key, TimeSpan maxAge, Func<Task<T>> func);
    }
}
