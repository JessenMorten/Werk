using System.Text.Json.Serialization;

namespace Werk.Services.AzureDevOps.ResponseModels
{
    public class MemberResponse
    {
        [JsonPropertyName("identity")]
        public MemberIdentity Identity { get; set; }

        [JsonPropertyName("isTeamAdmin")]
        public bool IsTeamAdmin { get; set; }

        public class MemberIdentity
        {
            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("_links")]
            public Links Links { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("uniqueName")]
            public string UniqueName { get; set; }

            [JsonPropertyName("imageUrl")]
            public string ImageUrl { get; set; }

            [JsonPropertyName("descriptor")]
            public string Descriptor { get; set; }
        }

        public class Links
        {
            [JsonPropertyName("avatar")]
            public Avatar Avatar { get; set; }
        }

        public class Avatar
        {
            [JsonPropertyName("href")]
            public string Href { get; set; }
        }
    }
}
