using System;
using System.IO;
using System.Linq;
using System.Reflection;
using GitReleaseNotes.Git;
using LibGit2Sharp;

namespace GitReleaseNotes
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = Args.Configuration.Configure<GitReleaseNotesArguments>().CreateAndBind(args);

            var workingDirectory = arguments.WorkingDirectory ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var gitDirectory = GitDirFinder.TreeWalkForGitDir(workingDirectory);
            if (string.IsNullOrEmpty(gitDirectory))
            {
                throw new Exception("Failed to find .git directory.");
            }

            Console.WriteLine("Git directory found at {0}", gitDirectory);

            var repositoryRoot = Directory.GetParent(gitDirectory).FullName;

            var gitHelper = new GitHelper();
            var gitRepo = new Repository(gitDirectory);
            var lastTag = new TaggedCommitFinder(gitRepo, gitHelper).GetLastTaggedCommit();
            var commitsToScan = gitRepo.Commits.TakeWhile(c => c != lastTag.Commit);

            if (arguments.Verbose)
            {
                Console.WriteLine("Scanning the following commits for issue numbers");
                foreach (var commit in commitsToScan)
                {
                    Console.WriteLine(commit.Message);
                }
            }
        }
    }
}
