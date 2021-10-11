using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Werk.Services.News
{
    public class NewsService : INewsService
    {
        private readonly IOptions<NewsOptions> _options;

        public NewsService(IOptions<NewsOptions> options)
        {
            _options = options;
        }

        public async Task<IEnumerable<News>> FetchNews(int maxCount = 5)
        {
            var news = new List<News>();

            foreach (var source in _options.Value.Sources)
            {
                news.Add(new News
                {
                    SourceName = source.Name,
                    Posts = (await FetchNewsPosts(source)).Take(maxCount)
                });
            }

            return news;
        }

        private async Task<IEnumerable<News.NewsPost>> FetchNewsPosts(NewsOptions.NewsSource source)
        {
            var result = new List<News.NewsPost>();

            try
            {
                var regex = new Regex(source.Regex);
                using var httpClient = new HttpClient();
                var data = await httpClient.GetStringAsync(source.Url);
                var matches = regex.Matches(data);

                foreach (var match in matches.ToList())
                {
                    var newPost = new News.NewsPost
                    {
                        Title = match.Groups.TryGetValue("title", out var title) ? title.Value : null,
                        Description = match.Groups.TryGetValue("description", out var description) ? description.Value : null,
                        Meta = match.Groups.TryGetValue("meta", out var meta) ? meta.Value : null,
                        Link = match.Groups.TryGetValue("link", out var link) ? link.Value : null,
                        CreatedAt = 
                            match.Groups.TryGetValue("createdAt", out var date) &&
                            DateTime.TryParse(date.Value, out var dateTime) ? dateTime : null
                    };

                    result.Add(newPost);
                }
            }
            catch
            {
                // Ignore
            }

            return result;
        }

        public Task<WerkServiceStatus> GetStatus()
        {
            var isConfigured = 
                _options.Value != null &&
                _options.Value.Sources != null &&
                _options.Value.Sources.Any();

            var status = isConfigured ? WerkServiceStatus.Ready : WerkServiceStatus.Disabled;

            return Task.FromResult(status);
        }
    }
}
