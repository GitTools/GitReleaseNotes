using System;
using System.IO;
using GitReleaseNotes.FileSystem;
using LibGit2Sharp;

namespace GitReleaseNotes.Git
{ 

    public class GitLocalRepositoryContextFactory : IGitRepositoryContextFactory
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly string workingDir;
        private readonly IFileSystem fileSystem;

        public GitLocalRepositoryContextFactory(string workingDir, IFileSystem fileSystem)
        {
            this.workingDir = workingDir;
            this.fileSystem = fileSystem;
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
            var context = new GitRepositoryContext(gitRepo, null, false, string.Empty, fileSystem);
            return context;
        }
    }

}

