using System;
using System.IO;
using GitReleaseNotes.FileSystem;
using LibGit2Sharp;

namespace GitReleaseNotes.Git
{
    public class GitRemoteRepositoryContextFactory : IGitRepositoryContextFactory
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly RemoteRepoArgs args;
        private readonly IFileSystem fileSystem;

        public GitRemoteRepositoryContextFactory(RemoteRepoArgs args, IFileSystem fileSystem)
        {
            this.args = args;
            this.fileSystem = fileSystem;
        }

        public GitRepositoryContext GetRepositoryContext()
        {
            args.Validate();

            var gitRootDirectory = Path.Combine(args.DestinationPath);
            var gitDirectory = Path.Combine(gitRootDirectory, ".git");

            var credentials = args.Credentials;

            if (fileSystem.DirectoryExists(gitDirectory))
            {
                Log.WriteLine("Git repository already exists, using existing instance from url '{0}'", args.Url);

                var repository = new Repository(gitDirectory);
                var repoContext = new GitRepositoryContext(repository, credentials, true, args.Url, fileSystem);
                return repoContext;
            }
            else
            {
                Log.WriteLine("Cloning git repository from url '{0}'", args.Url);

                var cloneOptions = new CloneOptions
                {
                    IsBare = true,
                    Checkout = false,
                    CredentialsProvider = (url, usernameFromUrl, types) => credentials
                };

                var repoPath = Repository.Clone(args.Url, gitDirectory, cloneOptions);
                var repository = new Repository(repoPath);
                var repoContext = new GitRepositoryContext(repository, credentials, true, args.Url, fileSystem);
                return repoContext;
            }
        }

        public class RemoteRepoArgs
        {
            public string DestinationPath { get; set; }
            public string Url { get; set; }
            public string Branch { get; set; }
            public Credentials Credentials { get; set; }

            internal void Validate()
            {
                if (string.IsNullOrEmpty(Url))
                {
                    throw new ArgumentException("Url of git repository must be specified.");
                }

                if (string.IsNullOrEmpty(DestinationPath))
                {
                    throw new ArgumentException("DestinationPath to place the cloned repository must be specified.");
                }
            }

            internal bool HasCredentials()
            {
                return Credentials != null;
            }
        }
    }
}