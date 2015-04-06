using GitReleaseNotes.Git;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public static class TestGitRepoUtils
    {

        public static string GetUniqueTempFolder(string parentFolderName)
        {
             var currentDir = Path.GetTempPath();   
             var repoDir = Path.Combine(currentDir, parentFolderName, Guid.NewGuid().ToString("N"));
             return repoDir;
        }

        public static IRepository CreateRepoWithBranch(string path, string branchName)
        {
            LibGit2Sharp.Repository.Init(path);
            Console.WriteLine("Created git repository at '{0}'", path);
                     
            var repo = new Repository(path);          

            // Let's move the HEAD to this branch to be created
            var branchInfo = new GitBranchNameInfo(branchName);

            repo.Refs.UpdateTarget("HEAD", branchInfo.GetCanonicalBranchName());
            // Create a commit against HEAD
            Commit c = GenerateCommit(repo);
            var branch = repo.Branches[branchName];
            if (branch == null)
            {
                Console.WriteLine("Branch was NULL!");
            }
            return repo;          
           
        }    

        public static IRepository CreateEmptyTestRepo(string path)
        {
            LibGit2Sharp.Repository.Init(path);
            Console.WriteLine("Created git repository at '{0}'", path);
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
