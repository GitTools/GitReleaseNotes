namespace GitReleaseNotes.IssueTrackers
{
    using LibGit2Sharp;

    public interface IIssueTrackerFactory
    {
        IIssueTracker CreateIssueTracker(Context context, IRepository repository);
    }
}