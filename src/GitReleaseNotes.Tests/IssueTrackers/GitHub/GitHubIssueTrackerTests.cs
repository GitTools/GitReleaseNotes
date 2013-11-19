using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.IssueTrackers.GitHub;
using LibGit2Sharp;
using NSubstitute;
using Octokit;
using Xunit;

namespace GitReleaseNotes.Tests.IssueTrackers.GitHub
{
    public class GitHubIssueTrackerTests
    {
        private readonly IGitHubClient _gitHubClient;
        private readonly GitHubIssueTracker _sut;
        private readonly GitReleaseNotesArguments _gitReleaseNotesArguments;
        private readonly IIssuesClient _issuesClient;

        public GitHubIssueTrackerTests()
        {
            _gitHubClient = Substitute.For<IGitHubClient>();
            _sut = new GitHubIssueTracker(new IssueNumberExtractor(), _gitHubClient);
            _gitReleaseNotesArguments = new GitReleaseNotesArguments
            {
                Repo = "Org/Repo",
                Token = "213"
            };
            _issuesClient = Substitute.For<IIssuesClient>();
        }

        [Fact]
        public void CreatesReleaseNotesForClosedGitHubIssues()
        {
            var commit = CreateCommit("Fixes #1", DateTimeOffset.Now.AddDays(-1));
            var commitsToScan = new[] { commit };
            _issuesClient
                .GetForRepository("Org", "Repo", Arg.Any<RepositoryIssueRequest>())
                .Returns(Task.FromResult<IReadOnlyList<Issue>>(new List<Issue>
                {
                    new Issue
                    {
                        Number = 1,
                        Title = "Issue Title"
                    }
                }.AsReadOnly()));
            _gitHubClient.Issue.Returns(_issuesClient);

            var releaseNotes = _sut.ScanCommitMessagesForReleaseNotes(_gitReleaseNotesArguments, commitsToScan);

            Assert.Equal("Issue Title", releaseNotes.ReleaseNoteItems[0].Title);
        }

        private static Commit CreateCommit(string message, DateTimeOffset when)
        {
            var commit = Substitute.For<Commit>();
            commit.Message.Returns(message);
            var commitSignature = new Signature("Jake", "", when);
            commit.Author.Returns(commitSignature);
            return commit;
        }
    }
}
