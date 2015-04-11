namespace GitReleaseNotes
{
    public class Context
    {
        public Context(IIssueTrackerContext issueTrackerContext)
        {
            Repository = new RepositoryContext();
            IssueTracker = issueTrackerContext;
        }

        public string WorkingDirectory { get; set; }
        public bool Verbose { get; set; }
        public string OutputFile { get; set; }
        public string Categories { get; set; }
        public string Version { get; set; }

        public bool AllTags { get; set; }
        public bool AllLabels { get; set; }

        public RepositoryContext Repository { get; private set; }
        public IIssueTrackerContext IssueTracker { get; private set; }
    }
}