namespace GitReleaseNotes.Website.Models.Api
{
    public static class ReleaseNotesRequestExtensions
    {
        public static Context ToContext(this ReleaseNotesRequest releaseNotesRequest)
        {
            IIssueTrackerContext issueTrackerContext = null;

            var lowercaseUrl = releaseNotesRequest.IssueTrackerUrl.ToLower();
            if (lowercaseUrl.Contains("bitbucket"))
            {
                issueTrackerContext = new BitBucketContext
                {
                    Url = releaseNotesRequest.IssueTrackerUrl
                };
            }

            if (lowercaseUrl.Contains("atlassian"))
            {
                issueTrackerContext = new JiraContext
                {
                    Url = releaseNotesRequest.IssueTrackerUrl
                };
            }

            if (lowercaseUrl.Contains("github"))
            {
                issueTrackerContext = new GitHubContext
                {
                    Url = releaseNotesRequest.IssueTrackerUrl
                };
            }

            if (lowercaseUrl.Contains("youtrack"))
            {
                issueTrackerContext = new YouTrackContext
                {
                    Url = releaseNotesRequest.IssueTrackerUrl
                };
            }

            var context = new Context(issueTrackerContext);

            context.Repository.Url = releaseNotesRequest.RepositoryUrl;
            context.Repository.Branch = releaseNotesRequest.RepositoryBranch;

            context.IssueTracker.ProjectId = releaseNotesRequest.IssueTrackerProjectId;

            return context;
        }
    }
}
