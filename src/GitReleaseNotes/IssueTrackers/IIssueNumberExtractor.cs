using System.Collections.Generic;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers
{
    public interface IIssueNumberExtractor
    {
        Dictionary<ReleaseInfo, List<string>> GetIssueNumbers(GitReleaseNotesArguments arguments, Dictionary<ReleaseInfo, List<Commit>> releases, string issueNumberRegexPattern);
    }
}