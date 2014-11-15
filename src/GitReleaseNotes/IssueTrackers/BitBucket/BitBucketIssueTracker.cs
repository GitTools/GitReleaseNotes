using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers.BitBucket
{
    public class BitBucketIssueTracker : IIssueTracker
    {
        private readonly IRepository gitRepository;

        private readonly GitReleaseNotesArguments arguments;
        private readonly BitBucketApi bitBucketApi;
        private readonly ILog log;
        private string accountName;
        private string repoSlug;
        private bool oauth;

        public BitBucketIssueTracker(IRepository gitRepository, BitBucketApi bitBucketApi, ILog log, GitReleaseNotesArguments arguments)
        {
            this.gitRepository = gitRepository;
            this.bitBucketApi = bitBucketApi;
            this.log = log;
            this.arguments = arguments;
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole()
        {
            if (!RemotePresentWhichMatches)
            {
                if (arguments.Repo == null)
                {
                    log.WriteLine("Bitbucket repository name must be specified [/Repo .../...]");
                    return false;
                }
                var repoParts = arguments.Repo.Split('/');

                if (repoParts.Length != 2)
                {
                    log.WriteLine("Bitbucket repository name should be in format Organisation/RepoName");
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

            if (string.IsNullOrEmpty(arguments.ConsumerKey) && string.IsNullOrEmpty(arguments.ConsumerSecretKey))
            {
                if (string.IsNullOrEmpty(arguments.Username))
                {
                    Console.WriteLine("/Username is a required to authenticate with BitBucket");
                    return false;
                }
                if (string.IsNullOrEmpty(arguments.Password))
                {
                    Console.WriteLine("/Password is a required to authenticate with BitBucket");
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(arguments.ConsumerKey))
                {
                    Console.WriteLine("/Consumer Key is a required to authenticate with BitBucket");
                    return false;
                }
                if (string.IsNullOrEmpty(arguments.ConsumerSecretKey))
                {
                    Console.WriteLine("/Consumer Secret Key is a required to authenticate with BitBucket");
                    return false;
                }
                oauth = true;
            }

            return true;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            return bitBucketApi.GetClosedIssues(arguments, since, accountName, repoSlug, oauth).ToArray();
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