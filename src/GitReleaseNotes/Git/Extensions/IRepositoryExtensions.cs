using System.Linq;
using GitTools.Git;
using LibGit2Sharp;

namespace GitReleaseNotes.Git
{
    public static class IRepositoryExtensions
    {
        public static ReleaseInfo GetCurrentReleaseInfo(this IRepository repository)
        {
            var lastTaggedCommit = repository.GetLastTaggedCommit();
            var head = repository.Head;
            if (head.Tip.Sha == lastTaggedCommit.Commit.Sha)
            {
                return new ReleaseInfo(null, null, lastTaggedCommit.Commit.Author.When);
            }

            var firstCommitAfterLastTag = head.Commits.TakeWhile(c => c.Id != lastTaggedCommit.Commit.Id).Last().Sha;
            return new ReleaseInfo(null, null, lastTaggedCommit.Commit.Author.When)
            {
                FirstCommit = firstCommitAfterLastTag,
                LastCommit = head.Tip.Sha
            };
        }
    }
}