using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitReleaseNotes.Git
{
    public class TaggedCommitFinder : ITaggedCommitFinder
    {
        private readonly Dictionary<string, TaggedCommit> _cache = new Dictionary<string, TaggedCommit>();
        private readonly IRepository _gitRepo;
        private readonly IGitHelper _gitHelper;

        public TaggedCommitFinder(IRepository gitRepo, IGitHelper gitHelper)
        {
            _gitRepo = gitRepo;
            _gitHelper = gitHelper;
        }

        public TaggedCommit GetLastTaggedCommit()
        {
            return GetTag(string.Empty);
        }

        public TaggedCommit FromFirstCommit()
        {
            var branch = GetMasterBranch(_gitRepo, _gitHelper);
            return new TaggedCommit(branch.Commits.Last(), "Initial Commit");
        }

        public TaggedCommit GetTag(string fromTag)
        {
            if (!_cache.ContainsKey(fromTag))
                _cache.Add(fromTag, GetLastTaggedCommit(_gitRepo, _gitHelper, t => string.IsNullOrEmpty(fromTag) || t.TagName == fromTag));

            return _cache[fromTag];
        }

        private static TaggedCommit GetLastTaggedCommit(IRepository gitRepo, IGitHelper gitHelper, Func<TaggedCommit, bool> filterTags)
        {
            var branch = GetMasterBranch(gitRepo, gitHelper);
            var tags = gitRepo.Tags
                .Select(t => new TaggedCommit((Commit)t.Target, t.Name))
                .Where(filterTags)
                .ToArray();
            var olderThan = branch.Tip.Committer.When;
            var lastTaggedCommit =
                branch.Commits.FirstOrDefault(c => c.Committer.When <= olderThan && tags.Any(a => a.Commit == c));

            if (lastTaggedCommit != null)
                return tags.Single(a => a.Commit.Sha == lastTaggedCommit.Sha);

            return new TaggedCommit(branch.Commits.Last(), "Initial Commit");
        }

        private static Branch GetMasterBranch(IRepository gitRepo, IGitHelper gitHelper)
        {
            return gitHelper.GetBranch(gitRepo, "master");
        }
    }
}