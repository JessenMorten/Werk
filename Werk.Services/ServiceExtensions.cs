using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Werk.Services.AzureDevOps;
using Werk.Services.YouTrack;
using Werk.Services.Cache;

namespace Werk.Services
{
    public static class ServiceExtensions
    {
        public static void AddWerkServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add cache service
            services.AddDistributedMemoryCache();
            services.AddTransient<ICacheService, CacheService>();

            // Add YouTrack service
            services.Configure<YouTrackOptions>(configuration.GetSection(nameof(YouTrackOptions)));
            services.AddSingleton<IYouTrackConnection, YouTrackConnection>();
            services.AddTransient<IYouTrackService, YouTrackService>();

            // Add Azure DevOps service
            services.Configure<AzureDevOpsOptions>(configuration.GetSection(nameof(AzureDevOpsOptions)));
            services.AddTransient<IAzureDevOpsService, AzureDevOpsService>();
        }
    }
}
