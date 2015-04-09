namespace GitReleaseNotes.IssueTrackers.GitHub
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using LibGit2Sharp;
    using Octokit;

    public class GitHubIssueTracker : IIssueTracker
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly Func<IGitHubClient> _gitHubClientFactory;
        private readonly Context _context;
        private readonly IRepository _gitRepository;

        public GitHubIssueTracker(IRepository gitRepository, Func<IGitHubClient> gitHubClientFactory, Context context)
        {
            _gitRepository = gitRepository;
            _context = context;
            _gitHubClientFactory = gitHubClientFactory;
        }

        public bool RemotePresentWhichMatches
        {
            get
            {
                return _gitRepository.Network.Remotes.Any(r => r.Url.ToLower().Contains("github.com"));
            }
        }

        public string DiffUrlFormat
        {
            get
            {
                string organisation;
                string repository;
                GetRepository(out organisation, out repository);

                return "https://github.com/" + organisation + "/" + repository + "/compare/{0}...{1}";
            }
        }

        private void GetRepository(out string organisation, out string repository)
        {
            if (RemotePresentWhichMatches)
            {
                if (TryRemote(out organisation, out repository, "upstream"))
                {
                    return;
                }

                if (TryRemote(out organisation, out repository, "origin"))
                {
                    return;
                }

                var remoteName = _gitRepository.Network.Remotes.First(r => r.Url.ToLower().Contains("github.com")).Name;
                if (TryRemote(out organisation, out repository, remoteName))
                {
                    return;
                }
            }

            var repoParts = _context.IssueTracker.ProjectId.Split('/');
            organisation = repoParts[0];
            repository = repoParts[1];
        }

        private bool TryRemote(out string organisation, out string repository, string remoteName)
        {
            var remote = _gitRepository.Network.Remotes[remoteName];
            if (remote != null && remote.Url.ToLower().Contains("github.com"))
            {
                var urlWithoutGitExtension = remote.Url.EndsWith(".git") ? remote.Url.Substring(0, remote.Url.Length - 4) : remote.Url;
                var match = Regex.Match(urlWithoutGitExtension, "github.com[/:](?<org>.*?)/(?<repo>.*)");
                if (match.Success)
                {
                    organisation = match.Groups["org"].Value;
                    repository = match.Groups["repo"].Value;
                    return true;
                }
            }

            organisation = null;
            repository = null;
            return false;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(IIssueTrackerContext context, DateTimeOffset? since)
        {
            string organisation;
            string repository;
            GetRepository(out organisation, out repository);

            var gitHubClient = _gitHubClientFactory();
            var forRepository = gitHubClient.Issue.GetForRepository(organisation, repository, new RepositoryIssueRequest
            {
                Filter = IssueFilter.All,
                Since = since,
                State = ItemState.Closed
            });

            var readOnlyList = forRepository.Result.Where(i => i.ClosedAt > since);

            var userCache = new Dictionary<string, User>();
            Func<User, string> getUserName = u =>
            {
                var login = u.Login;
                if (!userCache.ContainsKey(login))
                {
                    userCache.Add(login, string.IsNullOrEmpty(u.Name) ? gitHubClient.User.Get(login).Result : u);
                }

                var user = userCache[login];
                if (user != null)
                {
                    return user.Name;
                }

                return null;
            };
            return readOnlyList.Select(i => new OnlineIssue("#" + i.Number.ToString(CultureInfo.InvariantCulture), i.ClosedAt.Value)
            {
                HtmlUrl = i.HtmlUrl,
                Title = i.Title,
                IssueType = i.PullRequest == null ? IssueType.Issue : IssueType.PullRequest,
                Labels = i.Labels.Select(l => l.Name).ToArray(),
                Contributors = i.PullRequest == null ? new GitReleaseNotes.Contributor[0] : new[]
                {
                    new GitReleaseNotes.Contributor(getUserName(i.User), i.User.Login, i.User.HtmlUrl)
                }
            });
        }
    }
}