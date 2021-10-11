using System.Collections.Generic;

namespace Werk.Services.News
{
    public class NewsOptions
    {
        public IEnumerable<NewsSource> Sources { get; set; }

        public class NewsSource
        {
            public string Name { get; set; }

            public string Url { get; set; }

            public string Regex { get; set; }
        }
    }
}
