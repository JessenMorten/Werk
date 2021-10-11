using System;
using System.Collections.Generic;

namespace Werk.Services.News
{
    public class News
    {
        public string SourceName { get; set; }

        public IEnumerable<NewsPost> Posts { get; set; }

        public class NewsPost
        {
            public string Title { get; set; }

            public string Description { get; set; }

            public string Link { get; set; }

            public string Meta { get; set; }

            public DateTime? CreatedAt { get; set; }
        }
    }
}
