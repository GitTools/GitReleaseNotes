using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.IssueTrackers.GitHub;
using LibGit2Sharp;
using NSubstitute;
using Octokit;
using Xunit;
using Xunit.Extensions;

namespace GitReleaseNotes.Tests.IssueTrackers.GitHub
{
    public class GitHubIssueTrackerTests
    {
        private readonly IGitHubClient _gitHubClient;
        private readonly GitHubIssueTracker _sut;
        private readonly GitReleaseNotesArguments _gitReleaseNotesArguments;
        private readonly IIssuesClient _issuesClient;
        private readonly ILog _log;

        public GitHubIssueTrackerTests()
        {
            _log = Substitute.For<ILog>();
            _gitHubClient = Substitute.For<IGitHubClient>();
            _sut = new GitHubIssueTracker(new IssueNumberExtractor(), () => _gitHubClient, _log);
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
            var commitsToScan = new List<Commit> { commit };
            var toScan = new Dictionary<ReleaseInfo, List<Commit>>
            {
                {new ReleaseInfo(), commitsToScan}
            };
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

            var releaseNotes = _sut.ScanCommitMessagesForReleaseNotes(_gitReleaseNotesArguments, toScan);

            Assert.Equal("Issue Title", releaseNotes.Releases[0].ReleaseNoteItems[0].Title);
        }

        [Fact]
        public void ErrorLoggedWhenRepoIsNotSpecified()
        {
            var result = _sut.VerifyArgumentsAndWriteErrorsToConsole(new GitReleaseNotesArguments());

            Assert.False(result);
            _log.Received().WriteLine("GitHub repository name must be specified [/Repo .../...]");
        }

        [Theory]
        [InlineData("Foo", false)]
        [InlineData("Org/Repo", true)]
        [InlineData("Org/Repo/SomethingElse", false)]
        public void RepositoryMustBeInCorrectFormat(string repo, bool success)
        {
            var result = _sut.VerifyArgumentsAndWriteErrorsToConsole(new GitReleaseNotesArguments
            {
                Repo = repo,
                Token = "Foo"
            });

            if (success)
            {
                Assert.True(result);
            }
            else
            {
                Assert.False(result);
                _log.Received().WriteLine("GitHub repository name should be in format Organisation/RepoName");
            }
        }

        [Fact]
        public void MustSpecifyToken()
        {
            var result = _sut.VerifyArgumentsAndWriteErrorsToConsole(new GitReleaseNotesArguments
            {
                Repo = "Foo/Bar"
            });

            Assert.False(result);
            _log.Received().WriteLine("You must specify a GitHub Authentication token with the /Token argument");
        }

        [Fact]
        public void MustSpecifyVersionWhenPublishFlagIsSet()
        {
            var result = _sut.VerifyArgumentsAndWriteErrorsToConsole(new GitReleaseNotesArguments
            {
                Repo = "Foo/Bar",
                Token = "Baz",
                Publish = true
            });

            Assert.False(result);
            _log.Received().WriteLine("You must specifiy the version [/Version ...] (will be tag) when using the /Publish flag");
        }

        [Fact]
        public void CanCreateReleaseOnGitHub()
        {
            const string releaseNotesOutput = " - A thingy was fixed";
            _sut.PublishRelease(releaseNotesOutput, new GitReleaseNotesArguments
            {
                Repo = "Foo/Baz",
                Version = "1.2.0"
            });

            _gitHubClient.Release
                .Received()
                .CreateRelease("Foo", "Baz",
                    Arg.Is<ReleaseUpdate>(r => r.TagName == "1.2.0" && r.Body == releaseNotesOutput && r.Name == "1.2.0"));
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
