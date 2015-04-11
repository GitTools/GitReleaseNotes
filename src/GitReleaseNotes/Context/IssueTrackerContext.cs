namespace GitReleaseNotes
{
    public abstract class IssueTrackerContext : AuthenticationContext, IIssueTrackerContext
    {
        public string Url { get; set; }

        public string ProjectId { get; set; }
    }
}