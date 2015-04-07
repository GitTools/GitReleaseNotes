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

namespace GitReleaseNotes.Tests.IssueTrackers.GitHub
{
    public class GitHubIssueTrackerTests
    {
        private readonly GitReleaseNotesArguments arguments;
        private readonly IGitHubClient gitHubClient;
        private readonly IIssuesClient issuesClient;
        private readonly GitHubIssueTracker sut;
        private readonly IRepository repo;

        public GitHubIssueTrackerTests()
        {
            gitHubClient = Substitute.For<IGitHubClient>();
            issuesClient = Substitute.For<IIssuesClient>();
            gitHubClient.Issue.Returns(issuesClient);
            arguments = new GitReleaseNotesArguments
            {
                Repo = "Org/Repo",
                Token = "213"
            };

            var context = arguments.ToContext();

            repo = Substitute.For<IRepository>();
            repo.Network.Returns(new NetworkEx());

            sut = new GitHubIssueTracker(repo, () => gitHubClient, context);
        }

        [Fact]
        public void CreatesReleaseNotesForClosedGitHubIssues()
        {
            issuesClient
                .GetForRepository("Org", "Repo", Arg.Any<RepositoryIssueRequest>())
                .Returns(Task.FromResult<IReadOnlyList<Issue>>(new List<Issue>
                {
                    new Issue(null, null, 1, ItemState.Closed, "Issue Title", string.Empty, new TestUser("User", "Foo", "http://github.com/name"), 
                        new Collection<Label>(), null, null, 0, new PullRequest(), DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now)
                }.AsReadOnly()));

            var closedIssues = sut.GetClosedIssues(DateTimeOffset.Now.AddDays(-2));
            var onlineIssue = closedIssues.Single();
            onlineIssue.Title.ShouldBe("Issue Title");
            onlineIssue.Id.ShouldBe("#1");
            onlineIssue.Contributors.ShouldContain(c => c.Username == "User" && c.Name == "Foo" && c.Url == "http://github.com/name");
        }

        //[Fact]
        //public void ErrorLoggedWhenRepoIsNotSpecified()
        //{
        //    arguments.Repo = null;
        //    var result = sut.VerifyArgumentsAndWriteErrorsToLog();

        //    result.ShouldBe(false);
        //    Received().WriteLine("GitHub repository name must be specified [/Repo .../...]");
        //}

        //[Theory]
        //[InlineData("Foo", false)]
        //[InlineData("Org/Repo", true)]
        //[InlineData("Org/Repo/SomethingElse", false)]
        //public void RepositoryMustBeInCorrectFormat(string repository, bool success)
        //{
        //    arguments.Repo = repository;
        //    arguments.Token = "Foo";
        //    var result = sut.VerifyArgumentsAndWriteErrorsToLog();

        //    if (success)
        //    {
        //        result.ShouldBe(true);
        //    }
        //    else
        //    {
        //        result.ShouldBe(false);
        //        log.Received().WriteLine("GitHub repository name should be in format Organisation/RepoName");
        //    }
        //}

        [Fact]
        public void CanGetRepoFromRemote()
        {
            repo.Network.Remotes.Add("upstream", "http://github.com/Org/Repo.With.Dots");

            sut.GetClosedIssues(DateTimeOffset.Now.AddDays(-2));

            issuesClient.Received().GetForRepository("Org", "Repo.With.Dots", Arg.Any<RepositoryIssueRequest>());
        }
    }
}
