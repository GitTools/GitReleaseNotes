using System;
using System.Collections.Generic;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.IssueTrackers.Jira;
using LibGit2Sharp;
using NSubstitute;
using Xunit;

namespace GitReleaseNotes.Tests.IssueTrackers.Jira
{
    public class JiraIssueTrackerTests
    {
        private readonly IJiraApi _jiraApi;
        private readonly JiraIssueTracker _sut;

        public JiraIssueTrackerTests()
        {
            _jiraApi = Substitute.For<IJiraApi>();
            _sut = new JiraIssueTracker(new IssueNumberExtractor(), _jiraApi);
        }

        [Fact]
        public void CreatesReleaseNotesForClosedGitHubIssues()
        {
            var commit = CreateCommit("Fixes JIRA-5", DateTimeOffset.Now.AddDays(-1));
            var commitsToScan = new List<Commit> { commit };
            var toScan = new Dictionary<ReleaseInfo, List<Commit>>
            {
                {new ReleaseInfo(), commitsToScan}
            };
            _jiraApi
                .GetPotentialIssues(Arg.Any<Dictionary<ReleaseInfo, List<Commit>>>(), Arg.Any<GitReleaseNotesArguments>())
                .Returns(new List<JiraIssue>
                {
                    new JiraIssue
                    {
                        Id = "JIRA-5",
                        Name = "Issue Title"
                    }
                });

            var releaseNotes = _sut.ScanCommitMessagesForReleaseNotes(new GitReleaseNotesArguments
            {
                JiraServer = "http://my.jira.net",
                JiraProjectId = "JIRA"
            }, toScan);

            Assert.Equal("Issue Title", releaseNotes.Releases[0].ReleaseNoteItems[0].Title);
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