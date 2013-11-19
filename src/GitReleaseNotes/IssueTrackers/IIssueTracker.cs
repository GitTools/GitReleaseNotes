using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers
{
    public interface IIssueTracker
    {
        SemanticReleaseNotes ScanCommitMessagesForReleaseNotes(GitReleaseNotesArguments arguments, Commit[] commitsToScan);
        bool VerifyArgumentsAndWriteErrorsToConsole(GitReleaseNotesArguments arguments);
    }
}