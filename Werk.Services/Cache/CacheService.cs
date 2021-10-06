using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Werk.Services.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetOrSet<T>(string key, Func<Task<T>> func)
        {
            return await GetOrSet(key, TimeSpan.FromMinutes(1), func);
        }

        public async Task<T> GetOrSet<T>(string key, TimeSpan maxAge, Func<Task<T>> func)
        {
            T result;
            var json = await _cache.GetStringAsync(key);

            if (json is not null)
            {
                result = JsonSerializer.Deserialize<T>(json);
            }
            else
            {
                result = await func();
                json = JsonSerializer.Serialize(result);
                await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = maxAge
                });
            }

            return result;
        }
    }
}
