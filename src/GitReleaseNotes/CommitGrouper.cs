using System;
using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.Git;
using LibGit2Sharp;

namespace GitReleaseNotes
{
    public class CommitGrouper
    {
        public Dictionary<ReleaseInfo, List<Commit>> GetCommitsByRelease(IRepository gitRepo, TaggedCommit tagToStartFrom)
        {
            var currentRelease = new Tuple<ReleaseInfo, List<Commit>>(new ReleaseInfo(), new List<Commit>());
            var releases = new Dictionary<ReleaseInfo, List<Commit>> {{currentRelease.Item1, currentRelease.Item2}};
            var tagLookup = gitRepo.Tags.ToDictionary(t => t.Target.Sha, t => t);
            foreach (var commit in gitRepo.Commits.TakeWhile(c => tagToStartFrom == null || c != tagToStartFrom.Commit))
            {
                if (tagLookup.ContainsKey(commit.Sha))
                {
                    var tag = tagLookup[commit.Sha];
                    var releaseDate = ((Commit) tag.Target).Author.When;
                    currentRelease.Item1.PreviousReleaseDate = releaseDate;
                    var releaseInfo = new ReleaseInfo(tag.Name, releaseDate, null);
                    var commits = new List<Commit>();
                    currentRelease = new Tuple<ReleaseInfo, List<Commit>>(releaseInfo, commits);
                    releases.Add(releaseInfo, commits);
                }
                currentRelease.Item2.Add(commit);
            }

            return releases;
        }
    }
}