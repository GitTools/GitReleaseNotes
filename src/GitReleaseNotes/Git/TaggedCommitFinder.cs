using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Core.Compat;

namespace GitReleaseNotes.Git
{
    public class TaggedCommitFinder : ITaggedCommitFinder
    {
        private readonly Lazy<TaggedCommit> _lastTaggedRelease;

        public TaggedCommitFinder(IRepository gitRepo, IGitHelper gitHelper)
        {
            _lastTaggedRelease = new Lazy<TaggedCommit>(() => GetLastTaggedCommit(gitRepo, gitHelper));
        }

        public TaggedCommit GetLastTaggedCommit()
        {
            return _lastTaggedRelease.Value;
        }

        private static TaggedCommit GetLastTaggedCommit(IRepository gitRepo, IGitHelper gitHelper)
        {
            var branch = gitHelper.GetBranch(gitRepo, "master");
            var tags = gitRepo.Tags.Select(t => new TaggedCommit((Commit)t.Target, t.Name))
                .Where(a => a != null)
                .ToArray();
            var olderThan = branch.Tip.Committer.When;
            var lastTaggedCommit =
                branch.Commits.FirstOrDefault(c => c.Committer.When <= olderThan && tags.Any(a => a.Commit == c));

            if (lastTaggedCommit != null)
                return tags.Single(a => a.Commit.Sha == lastTaggedCommit.Sha);

            return new TaggedCommit(branch.Commits.Last(), "Initial Commit");
        }
    }
}