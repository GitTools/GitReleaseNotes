using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GitReleaseNotes.IssueTrackers.GitHub;
using LibGit2Sharp;
using NSubstitute;
using Octokit;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace GitReleaseNotes.Tests.IssueTrackers.GitHub
{
    public class GitHubIssueTrackerTests
    {
        private readonly GitReleaseNotesArguments _arguments;
        private readonly IGitHubClient _gitHubClient;
        private readonly IIssuesClient _issuesClient;
        private readonly GitHubIssueTracker _sut;
        private readonly ILog _log;
        private readonly IRepository _repo;

        public GitHubIssueTrackerTests()
        {
            _log = Substitute.For<ILog>();
            _gitHubClient = Substitute.For<IGitHubClient>();
            _issuesClient = Substitute.For<IIssuesClient>();
            _gitHubClient.Issue.Returns(_issuesClient);
            _arguments = new GitReleaseNotesArguments
            {
                Repo = "Org/Repo",
                Token = "213"
            };
            _repo = Substitute.For<IRepository>();
            _repo.Network.Returns(new NetworkEx());

            _sut = new GitHubIssueTracker(_repo, () => _gitHubClient, _log, _arguments);
        }

        [Fact]
        public void CreatesReleaseNotesForClosedGitHubIssues()
        {
            _issuesClient
                .GetForRepository("Org", "Repo", Arg.Any<RepositoryIssueRequest>())
                .Returns(Task.FromResult<IReadOnlyList<Issue>>(new List<Issue>
                {
                    new Issue
                    {
                        Number = 1,
                        Title = "Issue Title",
                        Labels = new Collection<Label>(),
                        ClosedAt = DateTimeOffset.Now,
                        PullRequest = new PullRequest(),
                        User = new User
                        {
                            Login = "User",
                            Name = "Foo",
                            HtmlUrl = "http://github.com/foo"
                        }
                    }
                }.AsReadOnly()));

            var closedIssues = _sut.GetClosedIssues(DateTimeOffset.Now.AddDays(-2));
            var onlineIssue = closedIssues.Single();
            onlineIssue.Title.ShouldBe("Issue Title");
            onlineIssue.Id.ShouldBe("#1");
            onlineIssue.Contributors.ShouldContain(c => c.Username == "User" && c.Name == "Foo" && c.Url == "http://github.com/foo");
        }

        [Fact]
        public void ErrorLoggedWhenRepoIsNotSpecified()
        {
            _arguments.Repo = null;
            var result = _sut.VerifyArgumentsAndWriteErrorsToConsole();

            result.ShouldBe(false);
            _log.Received().WriteLine("GitHub repository name must be specified [/Repo .../...]");
        }

        [Theory]
        [InlineData("Foo", false)]
        [InlineData("Org/Repo", true)]
        [InlineData("Org/Repo/SomethingElse", false)]
        public void RepositoryMustBeInCorrectFormat(string repo, bool success)
        {
            _arguments.Repo = repo;
            _arguments.Token = "Foo";
            var result = _sut.VerifyArgumentsAndWriteErrorsToConsole();

            if (success)
            {
                result.ShouldBe(true);
            }
            else
            {
                result.ShouldBe(false);
                _log.Received().WriteLine("GitHub repository name should be in format Organisation/RepoName");
            }
        }

        [Fact]
        public void CanGetRepoFromRemote()
        {
            _repo.Network.Remotes.Add("upstream", "http://github.com/Org/Repo.With.Dots");

            _sut.GetClosedIssues(DateTimeOffset.Now.AddDays(-2));

            _issuesClient.Received().GetForRepository("Org", "Repo.With.Dots", Arg.Any<RepositoryIssueRequest>());
        }

        [Fact]
        public void MustSpecifyVersionWhenPublishFlagIsSet()
        {
            _arguments.Repo = "Foo/Bar";
            _arguments.Token = "Baz";
            _arguments.Publish = true;
            var result = _sut.VerifyArgumentsAndWriteErrorsToConsole();

            result.ShouldBe(false);
            _log.Received().WriteLine("You must specifiy the version [/Version ...] (will be tag) when using the /Publish flag");
        }

        [Fact]
        public void CanCreateReleaseOnGitHub()
        {
            _arguments.Version = "1.2.0";
            _arguments.Repo = "Foo/Baz";
            const string releaseNotesOutput = " - A thingy was fixed";
            _sut.PublishRelease(releaseNotesOutput);

            var releaseUpdateSpec = Arg.Is<ReleaseUpdate>(r => r.TagName == "1.2.0" && r.Body == releaseNotesOutput && r.Name == "1.2.0");
            _gitHubClient.Release.Received().CreateRelease("Foo", "Baz", releaseUpdateSpec);
        }
    }
}
