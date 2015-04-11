namespace GitReleaseNotes
{
    public interface IIssueTrackerContext : IAuthenticationContext
    {
        string Url { get; set; }
        string ProjectId { get; set; }
    }
}