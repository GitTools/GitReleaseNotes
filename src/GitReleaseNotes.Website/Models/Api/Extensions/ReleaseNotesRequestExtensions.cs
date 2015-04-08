namespace GitReleaseNotes.Website.Models.Api
{
    public static class ReleaseNotesRequestExtensions
    {
        public static Context ToContext(this ReleaseNotesRequest releaseNotesRequest)
        {
            var context = new Context();

            context.ProjectId = releaseNotesRequest.IssueTrackerProjectId;

            context.Repository.Url = releaseNotesRequest.RepositoryUrl;
            context.Repository.Branch = releaseNotesRequest.RepositoryBranch;

            var lowercaseUrl = releaseNotesRequest.IssueTrackerUrl.ToLower();
            if (lowercaseUrl.Contains("bitbucket"))
            {
                context.BitBucket.Repo = releaseNotesRequest.IssueTrackerUrl;
            }

            if (lowercaseUrl.Contains("atlassian"))
            {
                context.Jira.JiraServer = releaseNotesRequest.IssueTrackerUrl;
            }

            if (lowercaseUrl.Contains("github"))
            {
                context.GitHub.Repo = releaseNotesRequest.IssueTrackerUrl;
            }

            if (lowercaseUrl.Contains("youtrack"))
            {
                context.YouTrack.YouTrackServer = releaseNotesRequest.IssueTrackerUrl;
            }

            return context;
        }
    }
}
