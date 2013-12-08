using System.Collections.Generic;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers
{
    public interface IIssueTracker
    {
        SemanticReleaseNotes ScanCommitMessagesForReleaseNotes(GitReleaseNotesArguments arguments, Dictionary<ReleaseInfo, List<Commit>> releases);
        bool VerifyArgumentsAndWriteErrorsToConsole(GitReleaseNotesArguments arguments);
        void PublishRelease(string releaseNotesOutput, GitReleaseNotesArguments arguments);
    }
}