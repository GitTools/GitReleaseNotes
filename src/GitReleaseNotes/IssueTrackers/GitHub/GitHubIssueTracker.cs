using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers.GitHub
{
    public class GitHubIssueTracker : IIssueTracker
    {
        private readonly IIssueNumberExtractor _issueNumberExtractor;

        public GitHubIssueTracker(IIssueNumberExtractor issueNumberExtractor)
        {
            _issueNumberExtractor = issueNumberExtractor;
        }

        public SemanticReleaseNotes ScanCommitMessagesForReleaseNotes(GitReleaseNotesArguments arguments, Commit[] commitsToScan)
        {
            var issueNumbersToScan = _issueNumberExtractor.GetIssueNumbers(arguments, commitsToScan, @"#(?<issueNumber>\d+)");

            return new SemanticReleaseNotes();
        }
    }
}