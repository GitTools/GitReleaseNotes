using System;
using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.Git;
using LibGit2Sharp;

namespace GitReleaseNotes
{
    public static class ReleaseFinder
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        public static List<ReleaseInfo> FindReleases(IRepository gitRepo, TaggedCommit tagToStartFrom, ReleaseInfo current)
        {
            var releases = new List<ReleaseInfo> { current };
            var tagLookup = TagsByShaMap(gitRepo);
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

        private static Dictionary<string, Tag> TagsByShaMap(IRepository gitRepo)
        {
            var tagLookup = new Dictionary<string, Tag>();

            foreach (var tag in gitRepo.Tags)
            {
                if (!tagLookup.ContainsKey(tag.Target.Sha))
                {
                    tagLookup.Add(tag.Target.Sha, tag);
                }
                else
                {
                    Log.WriteLine("Tag {0} not added to the release list, because a tag for that commit was added already.", tag);
                }
            }

            return tagLookup;
        }
    }
}