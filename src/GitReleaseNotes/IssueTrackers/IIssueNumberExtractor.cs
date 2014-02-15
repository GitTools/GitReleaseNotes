using System.Collections.Generic;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers
{
    public interface IIssueNumberExtractor
    {
        Dictionary<ReleaseInfo, List<string>> GetIssueNumbers(GitReleaseNotesArguments arguments, Dictionary<ReleaseInfo, List<Commit>> releases, Regex issueRegex);
    }
}