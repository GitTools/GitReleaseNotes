using System.Collections.Generic;
using GitReleaseNotes.IssueTrackers;
using LibGit2Sharp;

namespace GitReleaseNotes.GenerationStrategy
{
    public interface IReleaseNotesStrategy
    {
        SemanticReleaseNotes GetReleaseNotes(Dictionary<ReleaseInfo, List<Commit>> releases, GitReleaseNotesArguments tagToStartFrom, IIssueTracker issueTracker, string[] categories);
    }
}