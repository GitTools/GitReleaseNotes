using System;
using System.IO;
using GitReleaseNotes.Git;
using LibGit2Sharp;

namespace GitReleaseNotes.Tests
{
    public static class TestGitRepoUtils
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        public static string GetUniqueTempFolder(string parentFolderName)
        {
            var currentDir = Path.GetTempPath();
            var repoDir = Path.Combine(currentDir, parentFolderName, Guid.NewGuid().ToString("N"));
            return repoDir;
        }

        public static IRepository CreateRepoWithBranch(string path, string branchName)
        {
            Repository.Init(path);
            Log.WriteLine("Created git repository at '{0}'", path);

            var repo = new Repository(path);

            // Let's move the HEAD to this branch to be created
            var branchInfo = new GitBranchNameInfo(branchName);

            repo.Refs.UpdateTarget("HEAD", branchInfo.GetCanonicalBranchName());
            // Create a commit against HEAD
            var c = GenerateCommit(repo);
            var branch = repo.Branches[branchName];
            if (branch == null)
            {
                Log.WriteLine("Branch was NULL!");
            }

            return repo;
        }

        public static IRepository CreateEmptyTestRepo(string path)
        {
            Repository.Init(path);
            Log.WriteLine("Created git repository at '{0}'", path);
            return new Repository(path);
        }

        public static Commit GenerateCommit(IRepository repository, string comment = null)
        {
            var randomFile = Path.Combine(repository.Info.WorkingDirectory, Guid.NewGuid().ToString());
            File.WriteAllText(randomFile, string.Empty);
            comment = comment ?? "Test generated commit.";
            return CommitFile(repository, randomFile, comment);
        }

        public static Commit CommitFile(IRepository repo, string filePath, string comment)
        {
            repo.Stage(filePath);
            return repo.Commit(comment, SignatureNow(), SignatureNow());
        }

        public static Signature SignatureNow()
        {
            var dateTimeOffset = DateTimeOffset.Now;
            return Signature(dateTimeOffset);
        }

        public static Signature Signature(DateTimeOffset dateTimeOffset)
        {
            return new Signature("Billy", "billy@thesundancekid.com", dateTimeOffset);
        }
    }
}
