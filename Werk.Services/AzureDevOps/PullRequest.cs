using System;
using System.Text.Json.Serialization;

namespace Werk.Services.AzureDevOps
{
    public class PullRequest
    {
        [JsonPropertyName("repository")]
        public PullRequestRepository Repository { get; set; }

        [JsonPropertyName("pullRequestId")]
        public int PullRequestId { get; set; }

        [JsonPropertyName("codeReviewId")]
        public int CodeReviewId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("createdBy")]
        public Createdby CreatedBy { get; set; }

        [JsonPropertyName("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("sourceRefName")]
        public string SourceRefName { get; set; }

        [JsonPropertyName("targetRefName")]
        public string TargetRefName { get; set; }

        [JsonPropertyName("mergeStatus")]
        public string MergeStatus { get; set; }

        [JsonPropertyName("isDraft")]
        public bool IsDraft { get; set; }

        [JsonPropertyName("mergeId")]
        public string MergeId { get; set; }

        [JsonPropertyName("lastMergeSourceCommit")]
        public Commit LastMergeSourceCommit { get; set; }

        [JsonPropertyName("lastMergeTargetCommit")]
        public Commit LastMergeTargetCommit { get; set; }

        [JsonPropertyName("lastMergeCommit")]
        public Commit LastMergeCommit { get; set; }

        [JsonPropertyName("reviewers")]
        public object[] Reviewers { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("supportsIterations")]
        public bool SupportsIterations { get; set; }

        public class PullRequestRepository
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("project")]
            public Project Project { get; set; }
        }

        public class Project
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("state")]
            public string State { get; set; }

            [JsonPropertyName("visibility")]
            public string Visibility { get; set; }

            [JsonPropertyName("lastUpdateTime")]
            public DateTime LastUpdateTime { get; set; }
        }

        public class Createdby
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

        public class Commit
        {
            [JsonPropertyName("commitId")]
            public string CommitId { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
    }
}