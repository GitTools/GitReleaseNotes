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
                key += "_" + bitbucket.Repo;
            }

            var github = context.GitHub;
            if (!string.IsNullOrWhiteSpace(github.Repo))
            {
                key += "_" + github.Repo;
            }

            var jira = context.Jira;
            if (!string.IsNullOrWhiteSpace(jira.JiraServer))
            {
                key += "_" + jira.JiraServer;
            }

            var youtrack = context.YouTrack;
            if (!string.IsNullOrWhiteSpace(youtrack.YouTrackServer))
            {
                key += "_" + youtrack.YouTrackServer;
            }

            key = key.Replace("/", "_")
                .Replace("\\", "_")
                .Replace(":", "_");

            return key;
        }
    }
}
