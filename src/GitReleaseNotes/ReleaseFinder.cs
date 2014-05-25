using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.Git;
using LibGit2Sharp;

namespace GitReleaseNotes
{
    public static class ReleaseFinder
    {
        public static List<ReleaseInfo> FindReleases(IRepository gitRepo, TaggedCommit tagToStartFrom, ReleaseInfo current)
        {
            var releases = new List<ReleaseInfo> { current };
            var tagLookup = gitRepo.Tags.ToDictionary(t => t.Target.Sha, t => t);
            foreach (var commit in gitRepo.Commits.TakeWhile(c => tagToStartFrom == null || c != tagToStartFrom.Commit))
            {
                if (tagLookup.ContainsKey(commit.Sha))
                {
                    var tag = tagLookup[commit.Sha];
                    var releaseDate = ((Commit)tag.Target).Author.When;
                    current.PreviousReleaseDate = releaseDate;
                    current = new ReleaseInfo(tag.Name, releaseDate, null)
                    {
                        LastCommit = commit.Sha
                    };
                    releases.Add(current);
                }
                current.FirstCommit = commit.Sha;
            }

            return releases;
        }
    }
}