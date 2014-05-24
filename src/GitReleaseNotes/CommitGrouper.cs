using System;
using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.Git;
using LibGit2Sharp;

namespace GitReleaseNotes
{
    public static class CommitGrouper
    {
        public static List<ReleaseInfo> GetCommitsByRelease(IRepository gitRepo, TaggedCommit tagToStartFrom, ReleaseInfo current)
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
                    current = new ReleaseInfo(tag.Name, releaseDate, null, commit.Sha);
                    releases.Add(new ReleaseInfo(tag.Name, releaseDate, null, commit.Sha));
                }
                current.LastCommit = commit.Sha;
            }

            return releases;
        }
    }
}