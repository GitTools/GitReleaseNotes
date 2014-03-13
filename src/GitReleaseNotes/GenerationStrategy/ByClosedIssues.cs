using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.IssueTrackers;
using LibGit2Sharp;

namespace GitReleaseNotes.GenerationStrategy
{
    public class ByClosedIssues : IReleaseNotesStrategy
    {
        public SemanticReleaseNotes GetReleaseNotes(Dictionary<ReleaseInfo, List<Commit>> releases, GitReleaseNotesArguments tagToStartFrom,
            IIssueTracker issueTracker)
        {
            var closedIssues = issueTracker.GetClosedIssues(releases.Select(r => r.Key.When).Min()).ToArray();

            var semanticReleases = new List<SemanticRelease>();
            foreach (var release in releases)
            {
                var reloadLocal = release;
                var releaseNoteItems = closedIssues
                    .Where(i => 
                        (reloadLocal.Key.When == null || i.DateClosed < reloadLocal.Key.When) && 
                        (reloadLocal.Key.PreviousReleaseDate == null || i.DateClosed > reloadLocal.Key.PreviousReleaseDate))
                    .Select(i => new ReleaseNoteItem(i.Title, i.Id, i.HtmlUrl, i.Labels, i.DateClosed))
                    .ToList();
                semanticReleases.Add(new SemanticRelease(release.Key.Name, release.Key.When, releaseNoteItems, new ReleaseDiffInfo
                {
                    BeginningSha = release.Value.First().Sha.Substring(0, 10),
                    EndSha = release.Value.Last().Sha.Substring(0, 10)
                }));
            }

            return new SemanticReleaseNotes(semanticReleases);
        }
    }
}