using System;
using System.Text.Json.Serialization;

namespace Werk.Services.AzureDevOps.ResponseModels
{
    public class RepositoryResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("project")]
        public AzureDevOpsProject Project { get; set; }

        [JsonPropertyName("defaultBranch")]
        public string DefaultBranch { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("remoteUrl")]
        public string RemoteUrl { get; set; }

        [JsonPropertyName("sshUrl")]
        public string SshUrl { get; set; }

        [JsonPropertyName("webUrl")]
        public string WebUrl { get; set; }

        public class AzureDevOpsProject
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("state")]
            public string State { get; set; }

            [JsonPropertyName("revision")]
            public int Revision { get; set; }

            [JsonPropertyName("visibility")]
            public string Visibility { get; set; }

            [JsonPropertyName("lastUpdateTime")]
            public DateTime LastUpdateTime { get; set; }
        }
    }
}
