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
            var releases = ReleaseFinder.FindReleases(gitRepo, tagToStartFrom, currentReleaseInfo);

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
                var beginningSha = release.FirstCommit == null ? null : release.FirstCommit.Substring(0, 10);
                var endSha = release.LastCommit == null ? null : release.LastCommit.Substring(0, 10);
                semanticReleases.Add(new SemanticRelease(release.Name, release.When, releaseNoteItems, new ReleaseDiffInfo
                {
                    BeginningSha = beginningSha,
                    EndSha = endSha
                }));
            }

            return new SemanticReleaseNotes(semanticReleases, categories);
        }
    }
}