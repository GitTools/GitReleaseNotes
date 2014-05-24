using System;
using GitReleaseNotes.GenerationStrategy;
using GitReleaseNotes.Git;
using GitReleaseNotes.IssueTrackers;
using LibGit2Sharp;

namespace GitReleaseNotes
{
    public class ReleaseNotesGenerator
    {
        public static SemanticReleaseNotes GenerateReleaseNotes(IRepository gitRepo, IGitHelper gitHelper, GitReleaseNotesArguments arguments, IIssueTracker issueTracker, SemanticReleaseNotes previousReleaseNotes, string[] categories)
        {
            var taggedCommitFinder = new TaggedCommitFinder(gitRepo, gitHelper);

            var tagToStartFrom = arguments.AllTags
                ? taggedCommitFinder.FromFirstCommit()
                : taggedCommitFinder.GetLastTaggedCommit();

            var releases = new CommitGrouper().GetCommitsByRelease(
                gitRepo,
                tagToStartFrom,
                !string.IsNullOrEmpty(arguments.Version)
                    ? new ReleaseInfo(arguments.Version, DateTimeOffset.Now, tagToStartFrom.Commit.Author.When, null)
                    : null);

            IReleaseNotesStrategy generationStrategy;
            if (arguments.FromClosedIssues)
                generationStrategy = new ByClosedIssues();
            else if (arguments.FromMentionedIssues)
                generationStrategy = new ByMentionedCommits(new IssueNumberExtractor());
            else
                generationStrategy = new ByClosedIssues();

            return generationStrategy.GetReleaseNotes(releases, arguments, issueTracker, categories);
        }
    }
}