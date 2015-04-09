namespace GitReleaseNotes
{
    using GitReleaseNotes.IssueTrackers;

    public static class ContextExtensions
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        public static IssueTracker? GetIssueTracker(this Context context)
        {
            if (context.IssueTracker is BitBucketContext)
            {
                return IssueTracker.BitBucket;
            }

            if (context.IssueTracker is GitHubContext)
            {
                return IssueTracker.GitHub;
            }

            if (context.IssueTracker is JiraContext)
            {
                return IssueTracker.Jira;
            }

            if (context.IssueTracker is YouTrackContext)
            {
                return IssueTracker.YouTrack;
            }

            return null;
        }

        // TODO: Use IValidationContext
        public static bool Validate(this Context context)
        {
            if (!context.Repository.Validate())
            {
                return false;
            }

            if (!context.IssueTracker.Validate())
            {
                return false;
            }

            return true;
        }

        public static string GetContextKey(this Context context)
        {
            var key = string.Join("_", context.Repository.Url, context.Repository.Branch, context.IssueTracker.Url, context.IssueTracker.ProjectId);

            key = key.Replace("/", "_")
                .Replace("\\", "_")
                .Replace(":", "_");

            return key;
        }
    }
}
