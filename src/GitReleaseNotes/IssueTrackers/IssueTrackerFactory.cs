namespace GitReleaseNotes.IssueTrackers
{
    using GitHub;
    using Jira;
    using YouTrack;
    using LibGit2Sharp;
    using Octokit;

    public class IssueTrackerFactory : IIssueTrackerFactory
    {
        public IIssueTracker CreateIssueTracker(Context context, IRepository repository)
        {
            switch (context.GetIssueTracker())
            {
                case IssueTracker.BitBucket:
                    break;

                case IssueTracker.GitHub:
                    return new GitHubIssueTracker(repository, () =>
                    {
                        var gitHubClient = new GitHubClient(new ProductHeaderValue("GitReleaseNotes"));
                        if (context.IssueTracker.Token != null)
                        {
                            gitHubClient.Credentials = new Octokit.Credentials(context.IssueTracker.Token);
                        }

                        return gitHubClient;
                    }, context);

                case IssueTracker.Jira:
                    return new JiraIssueTracker(new JiraApi(), context);

                case IssueTracker.YouTrack:
                    return new YouTrackIssueTracker(new YouTrackApi(), context);
            }

            return null;
        }
    }
}