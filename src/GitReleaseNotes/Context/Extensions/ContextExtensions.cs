namespace GitReleaseNotes
{
    public static class ContextExtensions
    {
        public static string GetContextKey(this Context context)
        {
            var key = string.Format("{0}", context.ProjectId);

            var bitbucket = context.BitBucket;
            if (!string.IsNullOrWhiteSpace(bitbucket.Repo))
            {
                key += bitbucket.Repo;
            }

            var github = context.GitHub;
            if (!string.IsNullOrWhiteSpace(github.Repo))
            {
                key += github.Repo;
            }

            var jira = context.Jira;
            if (!string.IsNullOrWhiteSpace(jira.JiraServer))
            {
                key += jira.JiraServer;
            }

            var youtrack = context.YouTrack;
            if (!string.IsNullOrWhiteSpace(youtrack.YouTrackServer))
            {
                key += youtrack.YouTrackServer;
            }

            return key;
        }
    }
}
