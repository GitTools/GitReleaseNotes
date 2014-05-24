using System;
using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.Git;
using GitReleaseNotes.IssueTrackers;
using LibGit2Sharp;

namespace GitReleaseNotes
{
    public class ReleaseNotesGenerator
    {
        public static SemanticReleaseNotes GenerateReleaseNotes(IRepository gitRepo, IIssueTracker issueTracker, SemanticReleaseNotes previousReleaseNotes, string[] categories, TaggedCommit tagToStartFrom, ReleaseInfo currentReleaseInfo)
        {
            var releases = CommitGrouper.GetCommitsByRelease(
                gitRepo,
                tagToStartFrom,
                currentReleaseInfo);

            var closedIssues = issueTracker.GetClosedIssues(releases.Select(r => r.Key.PreviousReleaseDate).Min()).ToArray();

            var semanticReleases = new List<SemanticRelease>();
            foreach (var release in releases)
            {
                var reloadLocal = release;
                var releaseNoteItems = closedIssues
                    .Where(i => 
                        (reloadLocal.Key.When == null || i.DateClosed < reloadLocal.Key.When) && 
                        (reloadLocal.Key.PreviousReleaseDate == null || i.DateClosed > reloadLocal.Key.PreviousReleaseDate))
                    .Select(i => new ReleaseNoteItem(i.Title, i.Id, i.HtmlUrl, i.Labels, i.DateClosed, i.Contributors))
                    .ToList();
                semanticReleases.Add(new SemanticRelease(release.Key.Name, release.Key.When, releaseNoteItems, new ReleaseDiffInfo
                {
                    BeginningSha = release.Value.First().Sha.Substring(0, 10),
                    EndSha = release.Value.Last().Sha.Substring(0, 10)
                }));
            }

            return new SemanticReleaseNotes(semanticReleases, categories);
        }
    }
}