using System;
using System.Collections.Generic;
using System.Linq;
using Werk.Services.AzureDevOps.ResponseModels;

namespace Werk.Services.AzureDevOps
{
    public class PullRequest
    {
        public string Url { get; init; }

        public string Title { get; init; }

        public string Description { get; init; }

        public PullRequestMergeStatus MergeStatus { get; init; }

        public PullRequestCreator Creator { get; set; }

        public IEnumerable<PullRequestReviewer> Reviewers { get; set; }

        public DateTime CreationDate { get; set; }

        public string RepositoryName { get; set; }

        public string RepositoryUrl { get; set; }

        public PullRequest()
        {

        }

        public PullRequest(PullRequestResponse pullRequestResponse)
        {
            Url = pullRequestResponse.repository.webUrl + "/pullrequest/" + pullRequestResponse.pullRequestId;
            Title = pullRequestResponse.title;
            Description = pullRequestResponse.description;
            MergeStatus = new PullRequestMergeStatus
            {
                Value = pullRequestResponse.mergeStatus
            };
            Creator = new PullRequestCreator
            {
                DisplayName = pullRequestResponse.createdBy.displayName,
                AvatarUrl = pullRequestResponse.createdBy._links.avatar.href,
                UniqueName = pullRequestResponse.createdBy.uniqueName
            };
            Reviewers = (pullRequestResponse.reviewers ?? Enumerable.Empty<PullRequestResponse.Reviewer>())
                .Select(r => new PullRequestReviewer
                {
                    DisplayName = r.displayName,
                    AvatarUrl = r._links.avatar.href,
                    VoteValue = r.vote
                });
            CreationDate = pullRequestResponse.creationDate;
            RepositoryName = pullRequestResponse.repository.name;
            RepositoryUrl = pullRequestResponse.repository.webUrl;
        }

        public class PullRequestMergeStatus
        {
            public string Value { get; set; }

            public string Description => Value switch
            {
                "conflicts" => "Pull request merge failed due to conflicts",
                "failure" => "Pull request merge failed",
                "notSet" => "Status is not set",
                "queued" => "Pull request merge is queued",
                "rejectedByPolicy" => "Pull request merge rejected by policy",
                "succeeded" => "Pull request merge succeeded",
                _ => "Unknown merge status"
            };

            public bool Conflicts => Value == "conflicts";

            public bool Failure => Value == "failure";

            public bool NotSet => Value == "notSet";

            public bool Queued => Value == "queued";

            public bool RejectedByPolicy => Value == "rejectedByPolicy";

            public bool Succeeded => Value == "succeeded";
        }

        public class PullRequestCreator
        {
            public string DisplayName { get; init; }

            public string AvatarUrl { get; init; }

            public string UniqueName { get; set; }
        }

        public class PullRequestReviewer
        {
            public string DisplayName { get; init; }

            public string AvatarUrl { get; init; }

            public string VoteDescription => VoteValue switch
            {
                10 => "Approved",
                5 => "Approved with suggestions",
                0 => "No vote",
                -5 => "Waiting for author",
                -10 => "Rejected",
                _ => "Unknown"
            };

            public int VoteValue { get; set; }

            public bool Approved => VoteValue == 10;

            public bool ApprovedWithSuggestions => VoteValue == 5;

            public bool NoVote => VoteValue == 0;

            public bool WaitingForAuthor => VoteValue == -5;

            public bool Rejected => VoteValue == -10;
        }
    }
}
