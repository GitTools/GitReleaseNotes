using GitReleaseNotes.IssueTrackers;

namespace GitReleaseNotes
{
    public class Context
    {
        public Context()
        {
            Authentication = new AuthenticationContext();
            Repository = new RepositoryContext();

            GitHub = new GitHubContext();
            Jira = new JiraContext();
            YouTrack = new YouTrackContext();
            BitBucket = new BitBucketContext();
        }

        public string WorkingDirectory { get; set; }

        public bool Verbose { get; set; }

        public IssueTracker? IssueTracker { get; set; }

        public AuthenticationContext Authentication { get; private set; }
        public RepositoryContext Repository { get; private set; }

        public GitHubContext GitHub { get; private set; }
        public JiraContext Jira { get; private set; }
        public YouTrackContext YouTrack { get; private set; }
        public BitBucketContext BitBucket { get; private set; }

        public string ProjectId { get; set; }

        public string OutputFile { get; set; }

        public string Categories { get; set; }

        public string Version { get; set; }

        public bool AllTags { get; set; }

        public bool AllLabels { get; set; }
    }
}