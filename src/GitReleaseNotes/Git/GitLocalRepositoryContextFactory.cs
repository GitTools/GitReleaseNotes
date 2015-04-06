using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitReleaseNotes.Git
{ 

    public class GitLocalRepositoryContextFactory : IGitRepositoryContextFactory
    {
        private ILog logger;
        private string workingDir;

        public GitLocalRepositoryContextFactory(ILog logger, string workingDir)
        {
            this.logger = logger;
            this.workingDir = workingDir;
        }

        public GitRepositoryContext GetRepositoryContext()
        {
            // scan the working directory (default to current directory)
            var gitDirectory = GitDirFinder.TreeWalkForGitDir(workingDir);
            if (string.IsNullOrEmpty(gitDirectory))
            {
                throw new Exception("Failed to find a .git folder in the working directory.");
            }

            Console.WriteLine("Git directory found at {0}", gitDirectory);
            var repositoryRoot = Directory.GetParent(gitDirectory).FullName;
            var gitRepo = new Repository(gitDirectory);
            var context = new GitRepositoryContext(gitRepo, logger, null, false, string.Empty);
            return context;
        }
    }

}

