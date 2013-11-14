using GitReleaseNotes.IssueTrackers;
using LibGit2Sharp;
using NSubstitute;
using Xunit;

namespace GitReleaseNotes.Tests.IssueTrackers
{
    public class IssueNumberExtractorTests
    {
        [Fact]
        public void DiscoversIssueNumbersInCommits()
        {
            var commit = Substitute.For<Commit>();
            commit.Message.Returns("Fixing issue #123");
            var commit2 = Substitute.For<Commit>();
            commit2.Message.Returns("Fixing issue #51401");
            var commits = new[]
            {
                commit,
                commit2
            };

            var issueNumbers = new IssueNumberExtractor().GetIssueNumbers(new GitReleaseNotesArguments(), commits, "#(?<issueNumber>\\d+)");

            Assert.Contains("123", issueNumbers);
            Assert.Contains("51401", issueNumbers);
        }
    }
}