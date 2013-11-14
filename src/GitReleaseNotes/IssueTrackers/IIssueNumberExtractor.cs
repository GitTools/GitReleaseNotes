using System.Collections.Generic;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers
{
    public interface IIssueNumberExtractor
    {
        List<string> GetIssueNumbers(GitReleaseNotesArguments arguments, Commit[] commitsToScan, string issueNumberRegexPattern);
    }
}