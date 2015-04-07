using System;
using System.IO;
using LibGit2Sharp;

namespace GitReleaseNotes.Git
{

    public class GitRemoteRepositoryContextFactory : IGitRepositoryContextFactory
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly RemoteRepoArgs args;

        public GitRemoteRepositoryContextFactory(RemoteRepoArgs args)
        {
            this.args = args;
        }

        public GitRepositoryContext GetRepositoryContext()
        {
            args.Validate();

            var gitRootDirectory = Path.Combine(args.DestinationPath);
            var gitDirectory = Path.Combine(gitRootDirectory, ".git");
            if (Directory.Exists(gitDirectory))
            {
                Log.WriteLine("Deleting existing .git folder from '{0}' to force new checkout from url", gitDirectory);
                DeleteGitDirectory(gitDirectory);
            }

            var credentials = args.Credentials;

            Log.WriteLine("Retrieving git info from url '{0}'", args.Url);

            var cloneOptions = new CloneOptions();
            cloneOptions.IsBare = true;
            cloneOptions.Checkout = false;
            cloneOptions.CredentialsProvider = (url, usernameFromUrl, types) => credentials;

            var repoPath = Repository.Clone(args.Url, gitDirectory, cloneOptions);
            var repository = new Repository(repoPath);
            var repoContext = new GitRepositoryContext(repository, credentials, true, args.Url);
            return repoContext;
        }

        /// <summary>
        /// Deletes a .Git directory and all of it's contents.
        /// </summary>
        /// <param name="path"></param>
        private static void DeleteGitDirectory(string path)
        {
            var directory = new DirectoryInfo(path)
            {
                Attributes = FileAttributes.Normal
            };

            if (directory.Name != ".git")
            {
                throw new ArgumentException("Cannot delete a diretory that isn't a git repository.");
            }

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
        }

        public class RemoteRepoArgs
        {
            public string DestinationPath { get; set; }
            public string Url { get; set; }
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

