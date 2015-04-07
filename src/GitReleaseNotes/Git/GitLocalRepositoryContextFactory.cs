using System;
using System.IO;
using LibGit2Sharp;

namespace GitReleaseNotes.Git
{ 

    public class GitLocalRepositoryContextFactory : IGitRepositoryContextFactory
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private string workingDir;

        public GitLocalRepositoryContextFactory(string workingDir)
        {
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

            Log.WriteLine("Git directory found at {0}", gitDirectory);

            var gitRepo = new Repository(gitDirectory);
            var context = new GitRepositoryContext(gitRepo, null, false, string.Empty);
            return context;
        }
    }

}

