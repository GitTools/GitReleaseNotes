using System;
using GitReleaseNotes.IssueTrackers.Jira;
using LibGit2Sharp;
using NSubstitute;

namespace GitReleaseNotes.Tests.IssueTrackers.Jira
{
    public class JiraIssueTrackerTests
    {
        private readonly IJiraApi _jiraApi;
        private readonly JiraIssueTracker _sut;

        public JiraIssueTrackerTests()
        {
            _jiraApi = Substitute.For<IJiraApi>();
            _sut = new JiraIssueTracker(_jiraApi, new Context());
        }

        //[Fact]
        //public void CreatesReleaseNotesForClosedGitHubIssues()
        //{
        //    var commit = CreateCommit("Fixes JIRA-5", DateTimeOffset.Now.AddDays(-1));
        //    var commitsToScan = new List<Commit> { commit };
        //    var toScan = new Dictionary<ReleaseInfo, List<Commit>>
        //    {
        //        {new ReleaseInfo(), commitsToScan}
        //    };
        //    _jiraApi
        //        .GetClosedIssues(Arg.Any<GitReleaseNotesArguments>(), null)
        //        .Returns(new List<OnlineIssue>
        //        {
        //            new OnlineIssue
        //            {
        //                Id = "JIRA-5",
        //                Title = "Issue Title"
        //            }
        //        });

        //    var releaseNotes = _sut.ScanCommitMessagesForReleaseNotes(new GitReleaseNotesArguments
        //    {
        //        JiraServer = "http://my.jira.net",
        //        JiraProjectId = "JIRA"
        //    }, toScan);

        //    Assert.Equal("Issue Title", releaseNotes.Releases[0].ReleaseNoteLines[0].Title);
        //}

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