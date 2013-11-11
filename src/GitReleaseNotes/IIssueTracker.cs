using LibGit2Sharp;

namespace GitReleaseNotes
{
    public interface IIssueTracker
    {
        SemanticReleaseNotes ScanCommitMessagesForReleaseNotes(Commit[] commitsToScan);
    }
}