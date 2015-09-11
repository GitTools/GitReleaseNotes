using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApprovalTests;
using GitTools.IssueTrackers;
using LibGit2Sharp;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class ReleaseNotesGeneratorTests
    {
//        [Fact]
//        public async Task AllTagsWithNoCommitsOrIssuesAfterLastRelease()
//        {
//            var context = new Context(new JiraContext());

//            IRepository repo;
//            IIssueTracker issueTracker;
//            new TestDataCreator(new DateTimeOffset(2012, 1, 1, 0, 0, 0, new TimeSpan()))
//                .CreateRelease("0.1.0", "Issue1", "Issue2")
//                .CreateRelease("0.2.0", "Issue3")
//                .Build(out repo, out issueTracker);

//            var tagToStartFrom = GitRepositoryInfoFinder.GetFirstCommit(repo);
//            var currentReleaseInfo = GitRepositoryInfoFinder.GetCurrentReleaseInfo(repo); 

//            var releaseNotes = await ReleaseNotesGenerator.GenerateReleaseNotesAsync(context,
//                repo, issueTracker, new SemanticReleaseNotes(), new Categories(),
//                tagToStartFrom, currentReleaseInfo);

//            Approvals.Verify(releaseNotes.ToString(), Scrubber);
//        }

//        [Fact]
//        public async Task AllTags()
//        {
//            var context = new Context(new JiraContext());

//            IRepository repo;
//            IIssueTracker issueTracker;
//            new TestDataCreator(new DateTimeOffset(2012, 1, 1, 0, 0, 0, new TimeSpan()))
//                .CreateRelease("0.1.0", "Issue1", "Issue2")
//                .CreateRelease("0.2.0", "Issue3")
//                .AddIssues("Issue4")
//                .Build(out repo, out issueTracker);

//            var tagToStartFrom = GitRepositoryInfoFinder.GetFirstCommit(repo);
//            var currentReleaseInfo = GitRepositoryInfoFinder.GetCurrentReleaseInfo(repo); 

//            var releaseNotes = await ReleaseNotesGenerator.GenerateReleaseNotesAsync(context, 
//                repo, issueTracker, new SemanticReleaseNotes(), new Categories(),
//                tagToStartFrom, currentReleaseInfo);

//            Approvals.Verify(releaseNotes.ToString(), Scrubber);
//        }

//        [Fact(Skip = "To fix")]
//        public async Task AppendOnlyNewItems()
//        {
//            var context = new Context(new JiraContext());

//            IRepository repo;
//            IIssueTracker issueTracker;
//            new TestDataCreator(new DateTimeOffset(2012, 1, 1, 0, 0, 0, new TimeSpan()))
//                .CreateRelease("0.1.0", "Issue1", "Issue2")
//                .CreateRelease("0.2.0", "Issue3")
//                .AddIssues("Issue4")
//                .Build(out repo, out issueTracker);

//            var tagToStartFrom = GitRepositoryInfoFinder.GetFirstCommit(repo);
//            var currentReleaseInfo = GitRepositoryInfoFinder.GetCurrentReleaseInfo(repo);

//            var previousReleaseNotes = SemanticReleaseNotes.Parse(@"# vNext


//Commits: ...


//# 0.2.0 (05 January 2012)

// - [2] - Edited Issue3

//Commits:  AC39885536...CA74E870F2


//# 0.1.0 (03 January 2012)

// - [0] - Edited Issue1
// - [1] - Edited Issue2

//Commits: E413A880DB...F6924D7A0B");

//            var releaseNotes = await ReleaseNotesGenerator.GenerateReleaseNotesAsync(context,
//                repo, issueTracker, previousReleaseNotes, new Categories(),
//                tagToStartFrom, currentReleaseInfo);

//            Approvals.Verify(releaseNotes.ToString(), Scrubber);
//        }

//        [Fact(Skip = "To fix")]
//        public async Task KeepsCustomisations()
//        {
//            var context = new Context(new JiraContext());

//            IRepository repo;
//            IIssueTracker issueTracker;
//            new TestDataCreator(new DateTimeOffset(2012, 1, 1, 0, 0, 0, new TimeSpan()))
//                .CreateRelease("0.1.0", "Issue1", "Issue2")
//                .CreateRelease("0.2.0", "Issue3")
//                .Build(out repo, out issueTracker);

//            var tagToStartFrom = GitRepositoryInfoFinder.GetFirstCommit(repo);
//            var currentReleaseInfo = GitRepositoryInfoFinder.GetCurrentReleaseInfo(repo);

//            var previousReleaseNotes = SemanticReleaseNotes.Parse(@"# vNext


//Commits: ...


//# 0.2.0 (05 January 2012)

//**Note this release does some stuff!**


// - [2] - Edited Issue3

//Another comment

//Commits:  AC39885536...CA74E870F2


//# 0.1.0 (03 January 2012)

//## Features
// - [0] - Edited Issue1
// - [1] - Edited Issue2
// - [2] - Edited Issue3

//## Fixes
// - [3] - Edited Issue4
// - [4] - Edited Issue5

//This is a comment about the release

//Which spans multiple lines


//Commits: E413A880DB...F6924D7A0B");

//            var releaseNotes = await ReleaseNotesGenerator.GenerateReleaseNotesAsync(context,
//                repo, issueTracker, previousReleaseNotes, new Categories(),
//                tagToStartFrom, currentReleaseInfo);

//            Approvals.Verify(releaseNotes.ToString(), Scrubber);
//        }

        private static string Scrubber(string approval)
        {
            return Regex.Replace(approval, @".{10}\.\.\..{10}", "AAAAAAAAAA...BBBBBBBBBB");
        }
    }
}