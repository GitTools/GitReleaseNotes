using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.Git;
using GitReleaseNotes.IssueTrackers;
using LibGit2Sharp;

namespace GitReleaseNotes
{
    public static class ReleaseNotesGenerator
    {
        public static SemanticReleaseNotes GenerateReleaseNotes(IRepository gitRepo, IIssueTracker issueTracker, SemanticReleaseNotes previousReleaseNotes, string[] categories, TaggedCommit tagToStartFrom, ReleaseInfo currentReleaseInfo)
        {
            var releases = CommitGrouper.GetCommitsByRelease(gitRepo, tagToStartFrom, currentReleaseInfo);

            var closedIssues = issueTracker.GetClosedIssues(releases.Select(r => r.PreviousReleaseDate).Min()).ToArray();

            var semanticReleases = new List<SemanticRelease>();
            foreach (var release in releases)
            {
                var reloadLocal = release;
                var releaseNoteItems = closedIssues
                    .Where(i => 
                        (reloadLocal.When == null || i.DateClosed < reloadLocal.When) && 
                        (reloadLocal.PreviousReleaseDate == null || i.DateClosed > reloadLocal.PreviousReleaseDate))
                    .Select(i => new ReleaseNoteItem(i.Title, i.Id, i.HtmlUrl, i.Labels, i.DateClosed, i.Contributors))
                    .ToList();
                semanticReleases.Add(new SemanticRelease(release.Name, release.When, releaseNoteItems, new ReleaseDiffInfo
                {
                    BeginningSha = release.FirstCommit.Substring(0, 10),
                    EndSha = release.LastCommit.Substring(0, 10)
                }));
            }

            return new SemanticReleaseNotes(semanticReleases, categories);
        }
    }
}