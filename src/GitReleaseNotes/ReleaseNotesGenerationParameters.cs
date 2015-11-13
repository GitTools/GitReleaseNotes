using GitTools.Git;

namespace GitReleaseNotes
{
    public class ReleaseNotesGenerationParameters
    {
        public ReleaseNotesGenerationParameters()
        {
            Repository = new RepositoryInfo();
            IssueTracker = new IssueTrackerParameters();
        }

        public string Categories { get; set; }
        public string Version { get; set; }

        public bool AllTags { get; set; }
        public bool AllLabels { get; set; }
        public RepositoryInfo Repository { get; private set; }
        public IssueTrackerParameters IssueTracker { get; private set; }

        public string WorkingDirectory { get; set; }
    }
}