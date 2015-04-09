namespace GitReleaseNotes.IssueTrackers
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using LibGit2Sharp;

    public interface IIssueNumberExtractor
    {
        Dictionary<ReleaseInfo, List<string>> GetIssueNumbers(GitReleaseNotesArguments arguments, Dictionary<ReleaseInfo, List<Commit>> releases, Regex issueRegex);
    }
}