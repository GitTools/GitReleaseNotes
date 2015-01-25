using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitReleaseNotes.Git
{  

    public class GitRemoteRepositoryContextFactory : IGitRepositoryContextFactory
    {
        private ILog logger;
        private RemoteRepoArgs args;

        public GitRemoteRepositoryContextFactory(ILog logger, RemoteRepoArgs args)
        {
            this.logger = logger;
            this.args = args;
        }

        public GitRepositoryContext GetRepositoryContext()
        {
            args.Validate();

            var gitRootDirectory = Path.Combine(args.DestinationPath);
            var gitDirectory = Path.Combine(gitRootDirectory, ".git");
            if (Directory.Exists(gitRootDirectory))
            {
                logger.WriteLine(string.Format("Deleting existing .git folder from '{0}' to force new checkout from url", gitRootDirectory));
                DeleteGitDirectory(gitRootDirectory);
            }

            Credentials credentials = args.Credentials;            

            logger.WriteLine(string.Format("Retrieving git info from url '{0}'", args.Url));

            var cloneOptions = new CloneOptions();
            cloneOptions.IsBare = true;
            cloneOptions.Checkout = false;
            cloneOptions.CredentialsProvider = (url, usernameFromUrl, types) => credentials;

            var repoPath = Repository.Clone(args.Url, gitDirectory, cloneOptions);
            var repository = new Repository(repoPath);
            var repoContext = new GitRepositoryContext(repository, logger, credentials, true, args.Url);          
            return repoContext;
        }

        /// <summary>
        /// Deletes a .Git directory and all of it's contents.
        /// </summary>
        /// <param name="path"></param>
        private static void DeleteGitDirectory(string path)
        {
            var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };
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

