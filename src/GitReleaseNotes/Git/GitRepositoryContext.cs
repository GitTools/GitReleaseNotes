using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitReleaseNotes.Git
{
    /// <summary>
    /// Provides a useful wrapper around an <see cref="IRepository" instance. />
    /// </summary>
    public sealed class GitRepositoryContext : IDisposable
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private IRepository repository;
        private bool isRemote;
        private Credentials credentials;
        private string repoUrl;

        public GitRepositoryContext(IRepository repository, Credentials credentials, bool isRemote, string repoUrl)
        {
            this.repository = repository;
            this.isRemote = isRemote;
            this.credentials = credentials;
            this.repoUrl = repoUrl;           
        }

        public IRepository Repository { get { return repository; } }

        public bool IsRemote { get { return isRemote; } }

        /// <summary>
        /// Prepares the git repository for first use.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="credentials"></param>
        /// <param name="branchName"></param>
        /// <param name="url"></param>
        public void PrepareRemoteRepoForUse(string branchName)
        {
            // Normalize (download branches) before using the branch
            this.NormalizeGitDirectory();

            string targetBranch = branchName;
            if (string.IsNullOrWhiteSpace(branchName))
            {
                targetBranch = Repository.Head.Name;
            }
            var branchNameInfo = new GitBranchNameInfo(targetBranch);

            Reference newHead = null;
            var localReference = GetLocalReference(branchNameInfo);
            if (localReference != null)
            {
                newHead = localReference;
            }

            if (newHead == null)
            {
                var remoteReference = GetRemoteReference(branchNameInfo);
                if (remoteReference != null)
                {
                    Repository.Network.Fetch(repoUrl, new[]
                            {
                                string.Format("{0}:{1}", remoteReference.CanonicalName, targetBranch)
                            });

                    newHead = Repository.Refs[string.Format("refs/heads/{0}", targetBranch)];
                }
            }

            if (newHead != null)
            {
                Log.WriteLine("Switching to branch '{0}'", targetBranch);
                Repository.Refs.UpdateTarget(Repository.Refs.Head, newHead);
            }
        }

        public void CheckoutFilesIfExist(params string[] fileNames)
        {
            if (fileNames == null || fileNames.Length == 0)
            {
                return;
            }

            Log.WriteLine("Checking out files that might be needed later.");

            foreach (var fileName in fileNames)
            {
                try
                {
                    Log.WriteLine("  Trying to check out '{0}'", fileName);

                    var headBranch = repository.Head;
                    var tip = headBranch.Tip;

                    var treeEntry = tip[fileName];
                    if (treeEntry == null)
                    {
                        continue;
                    }

                    var fullPath = Path.Combine(GetRepositoryDirectory(), fileName);
                    using (var stream = ((Blob)treeEntry.Target).GetContentStream())
                    {
                        using (var streamReader = new BinaryReader(stream))
                        {
                            File.WriteAllBytes(fullPath, streamReader.ReadBytes((int)stream.Length));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine(" An error occurred while checking out '{0}': '{1}'", fileName, ex.Message);
                }
            }
        }

        public string GetRepositoryDirectory(bool omitGitPostFix = true)
        {
            var gitDirectory = repository.Info.Path;

            gitDirectory = gitDirectory.TrimEnd('\\');

            if (omitGitPostFix && gitDirectory.EndsWith(".git"))
            {
                gitDirectory = gitDirectory.Substring(0, gitDirectory.Length - ".git".Length);
                gitDirectory = gitDirectory.TrimEnd('\\');
            }

            return gitDirectory;
        }

        public Reference GetLocalReference(GitBranchNameInfo branchNameInfo)
        {
            var targetBranchName = branchNameInfo.GetCanonicalBranchName();
            return repository.Refs.FirstOrDefault(localRef => string.Equals(localRef.CanonicalName, targetBranchName));
        }

        public DirectReference GetRemoteReference(GitBranchNameInfo branchNameInfo)
        {
            var targetBranchName = branchNameInfo.GetCanonicalBranchName();
            var remoteReferences = repository.Network.ListReferences(repoUrl);
            return remoteReferences.FirstOrDefault(remoteRef => string.Equals(remoteRef.CanonicalName, targetBranchName));
        }       

        public void Dispose()
        {
            if (repository != null)
            {
                repository.Dispose();
                repository = null;
            }
        }

        // private helper methods.

        private void NormalizeGitDirectory()
        {
            var remote = EnsureSingleRemoteIsDefined();
            EnsureRepoHasRefSpecs(remote);

            Log.WriteLine("Fetching from remote '{0}' using the following refspecs: {1}.",
                remote.Name, string.Join(", ", remote.FetchRefSpecs.Select(r => r.Specification)));

            var fetchOptions = new FetchOptions();
            fetchOptions.CredentialsProvider = (url, user, types) => credentials;
            Repository.Network.Fetch(remote, fetchOptions);

            CreateMissingLocalBranchesFromRemoteTrackingOnes(remote.Name);
            var headSha = Repository.Refs.Head.TargetIdentifier;

            if (!Repository.Info.IsHeadDetached)
            {
                Log.WriteLine("HEAD points at branch '{0}'.", headSha);
                return;
            }

            Log.WriteLine("HEAD is detached and points at commit '{0}'.", headSha);

            // In order to decide whether a fake branch is required or not, first check to see if any local branches have the same commit SHA of the head SHA.
            // If they do, go ahead and checkout that branch
            // If no, go ahead and check out a new branch, using the known commit SHA as the pointer
            var localBranchesWhereCommitShaIsHead = Repository.Branches.Where(b => !b.IsRemote && b.Tip.Sha == headSha).ToList();

            if (localBranchesWhereCommitShaIsHead.Count > 1)
            {
                var names = string.Join(", ", localBranchesWhereCommitShaIsHead.Select(r => r.CanonicalName));
                var message = string.Format("Found more than one local branch pointing at the commit '{0}'. Unable to determine which one to use ({1}).", headSha, names);
                throw new InvalidOperationException(message);
            }

            if (localBranchesWhereCommitShaIsHead.Count == 0)
            {
                Log.WriteLine("No local branch pointing at the commit '{0}'. Fake branch needs to be created.", headSha);
                CreateFakeBranchPointingAtThePullRequestTip();
            }
            else
            {
                Log.WriteLine("Checking out local branch 'refs/heads/{0}'.", localBranchesWhereCommitShaIsHead[0].Name);
                Repository.Branches[localBranchesWhereCommitShaIsHead[0].Name].Checkout();
            }
        }

        private void CreateFakeBranchPointingAtThePullRequestTip()
        {
            var remote = Repository.Network.Remotes.Single();

            var remoteTips = this.GetRemoteTips(remote);

            var headTipSha = Repository.Head.Tip.Sha;
            var refs = remoteTips.Where(r => r.TargetIdentifier == headTipSha).ToList();

            if (refs.Count == 0)
            {
                var message = string.Format("Couldn't find any remote tips from remote '{0}' pointing at the commit '{1}'.", remote.Url, headTipSha);
                throw new GitReleaseNotesException(message);
            }

            if (refs.Count > 1)
            {
                var names = string.Join(", ", refs.Select(r => r.CanonicalName));
                var message = string.Format("Found more than one remote tip from remote '{0}' pointing at the commit '{1}'. Unable to determine which one to use ({2}).", remote.Url, headTipSha, names);
                throw new GitReleaseNotesException(message);
            }

            var canonicalName = refs[0].CanonicalName;
            Log.WriteLine("Found remote tip '{0}' pointing at the commit '{1}'.", canonicalName, headTipSha);

            if (!canonicalName.StartsWith("refs/pull/") && !canonicalName.StartsWith("refs/pull-requests/"))
            {
                var message = string.Format("Remote tip '{0}' from remote '{1}' doesn't look like a valid pull request.", canonicalName, remote.Url);
                throw new GitReleaseNotesException(message);
            }

            var fakeBranchName = canonicalName.Replace("refs/pull/", "refs/heads/pull/").Replace("refs/pull-requests/", "refs/heads/pull-requests/");

            Log.WriteLine("Creating fake local branch '{0}'.", fakeBranchName);
            Repository.Refs.Add(fakeBranchName, new ObjectId(headTipSha));

            Log.WriteLine("Checking local branch '{0}' out.", fakeBranchName);
            Repository.Checkout(fakeBranchName);
        }

        private IEnumerable<DirectReference> GetRemoteTips(Remote remote)
        {
            return Repository.Network.ListReferences(remote, (url, fromUrl, types) => credentials);
        }

        private void CreateMissingLocalBranchesFromRemoteTrackingOnes(string remoteName)
        {
            var prefix = string.Format("refs/remotes/{0}/", remoteName);
            var remoteHeadCanonicalName = string.Format("{0}{1}", prefix, "HEAD");

            foreach (var remoteTrackingReference in Repository.Refs.FromGlob(prefix + "*").Where(r => r.CanonicalName != remoteHeadCanonicalName))
            {
                var localCanonicalName = "refs/heads/" + remoteTrackingReference.CanonicalName.Substring(prefix.Length);

                if (Repository.Refs.Any(x => x.CanonicalName == localCanonicalName))
                {
                    Log.WriteLine("Skipping local branch creation since it already exists '{0}'.", remoteTrackingReference.CanonicalName);
                    continue;
                }

                Log.WriteLine("Creating local branch from remote tracking '{0}'.", remoteTrackingReference.CanonicalName);

                var symbolicReference = remoteTrackingReference as SymbolicReference;
                if (symbolicReference == null)
                {
                    Repository.Refs.Add(localCanonicalName, new ObjectId(remoteTrackingReference.TargetIdentifier), true);
                }
                else
                {
                    Repository.Refs.Add(localCanonicalName, new ObjectId(symbolicReference.ResolveToDirectReference().TargetIdentifier), true);
                }
            }
        }

        private void EnsureRepoHasRefSpecs(Remote remote)
        {
            if (remote.FetchRefSpecs.Any(r => r.Source == "refs/heads/*"))
            {
                return;
            }

            var allBranchesFetchRefSpec = string.Format("+refs/heads/*:refs/remotes/{0}/*", remote.Name);
            Log.WriteLine("Adding refspec: {0}", allBranchesFetchRefSpec);
            Repository.Network.Remotes.Update(remote, r => r.FetchRefSpecs.Add(allBranchesFetchRefSpec));
        }

        private Remote EnsureSingleRemoteIsDefined()
        {
            var remotes = Repository.Network.Remotes;
            var howMany = remotes.Count();

            if (howMany == 1)
            {
                var remote = remotes.Single();
                Log.WriteLine("One remote found ({0} -> '{1}').", remote.Name, remote.Url);
                return remote;
            }

            var message = string.Format("{0} remote(s) have been detected. When being run on a TeamCity agent, the Git repository is expected to bear one (and no more than one) remote.", howMany);
            throw new GitReleaseNotesException(message);
        }
    }
}
