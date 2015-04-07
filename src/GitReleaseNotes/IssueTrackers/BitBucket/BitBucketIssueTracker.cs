using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers.BitBucket
{
    public class BitBucketIssueTracker : IIssueTracker
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly IRepository gitRepository;

        private readonly Context context;
        private readonly BitBucketApi bitBucketApi;
        private string accountName;
        private string repoSlug;
        private bool oauth;

        public BitBucketIssueTracker(IRepository gitRepository, BitBucketApi bitBucketApi, Context context)
        {
            this.gitRepository = gitRepository;
            this.bitBucketApi = bitBucketApi;
            this.context = context;
        }

        public bool VerifyArgumentsAndWriteErrorsToLog()
        {
            if (!RemotePresentWhichMatches)
            {
                var repo = context.BitBucket.Repo;
                if (repo == null)
                {
                    Log.WriteLine("Bitbucket repository name must be specified [/Repo .../...]");
                    return false;
                }

                var repoParts = repo.Split('/');
                if (repoParts.Length != 2)
                {
                    Log.WriteLine("Bitbucket repository name should be in format Organisation/RepoName");
                    return false;
                }

                accountName = repoParts[0];
                repoSlug = repoParts[1];
            }
            else
            {
                var remotes = gitRepository.Network.Remotes.Where(r => r.Url.ToLower().Contains("bitbucket.org"));
                var remoteUrl = remotes.First().Url;
                var split = remoteUrl.Split(new[] {'/', '.'});
                accountName = split[4];
                repoSlug = split[5];
            }

            if (string.IsNullOrEmpty(context.BitBucket.ConsumerKey) && string.IsNullOrEmpty(context.BitBucket.ConsumerSecretKey))
            {
                if (string.IsNullOrEmpty(context.Authentication.Username))
                {
                    Log.WriteLine("/Username is a required to authenticate with BitBucket");
                    return false;
                }

                if (string.IsNullOrEmpty(context.Authentication.Password))
                {
                    Log.WriteLine("/Password is a required to authenticate with BitBucket");
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(context.BitBucket.ConsumerKey))
                {
                    Log.WriteLine("/Consumer Key is a required to authenticate with BitBucket");
                    return false;
                }

                if (string.IsNullOrEmpty(context.BitBucket.ConsumerSecretKey))
                {
                    Log.WriteLine("/Consumer Secret Key is a required to authenticate with BitBucket");
                    return false;
                }

                oauth = true;
            }

            return true;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            return bitBucketApi.GetClosedIssues(context, since, accountName, repoSlug, oauth).ToArray();
        }

        public bool RemotePresentWhichMatches
        {
            get { return gitRepository.Network.Remotes.Any(r => r.Url.ToLower().Contains("bitbucket.org")); }
        }

        public string DiffUrlFormat
        {
            get { return string.Empty; }
        }
    }
}