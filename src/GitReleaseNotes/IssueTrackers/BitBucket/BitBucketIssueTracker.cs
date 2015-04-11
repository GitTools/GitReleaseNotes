using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers.BitBucket
{
    public class BitBucketIssueTracker : IIssueTracker
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly IRepository _gitRepository;

        private readonly Context _context;
        private readonly BitBucketApi _bitBucketApi;
        private string _accountName;
        private string _repoSlug;
        private bool _oauth;

        public BitBucketIssueTracker(IRepository gitRepository, BitBucketApi bitBucketApi, Context context)
        {
            _gitRepository = gitRepository;
            _bitBucketApi = bitBucketApi;
            _context = context;

            if (!RemotePresentWhichMatches)
            {
                var repo = _context.IssueTracker.Url;
                if (repo == null)
                {
                    Log.WriteLine("Bitbucket repository name must be specified [/Repo .../...]");
                    return;
                }

                var repoParts = repo.Split('/');
                if (repoParts.Length != 2)
                {
                    Log.WriteLine("Bitbucket repository name should be in format Organisation/RepoName");
                    return;
                }

                _accountName = repoParts[0];
                _repoSlug = repoParts[1];
            }
            else
            {
                var remotes = _gitRepository.Network.Remotes.Where(r => r.Url.ToLower().Contains("bitbucket.org"));
                var remoteUrl = remotes.First().Url;
                var split = remoteUrl.Split('/', '.');
                _accountName = split[4];
                _repoSlug = split[5];
            }

            // Assume oauth first
            _oauth = true;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(IIssueTrackerContext context, DateTimeOffset? since)
        {
            return _bitBucketApi.GetClosedIssues(context, since, _accountName, _repoSlug, _oauth).ToArray();
        }

        public bool RemotePresentWhichMatches
        {
            get { return _gitRepository.Network.Remotes.Any(r => r.Url.ToLower().Contains("bitbucket.org")); }
        }

        public string DiffUrlFormat
        {
            get { return string.Empty; }
        }
    }
}