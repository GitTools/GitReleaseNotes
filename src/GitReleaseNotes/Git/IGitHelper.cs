using LibGit2Sharp;

namespace GitReleaseNotes.Git
{
    public interface IGitHelper
    {
        int NumberOfCommitsOnBranchSinceCommit(Branch branch, Commit commit);
        Branch GetBranch(IRepository repository, string name);
    }
}