using GitTools.IssueTrackers;

namespace GitReleaseNotes
{
    public class Context : GitTools.ContextBase
    {
        public Context(IIssueTrackerContext issueTrackerContext)
        {
            IssueTracker = issueTrackerContext;
        }

        public bool Verbose { get; set; }
        public string OutputFile { get; set; }
        public string Categories { get; set; }
        public string Version { get; set; }

        public bool AllTags { get; set; }
        public bool AllLabels { get; set; }
        
        public IIssueTrackerContext IssueTracker { get; private set; }
    }
}