using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Octokit;

namespace GitReleaseNotes.IssueTrackers.GitHub
{
    public class GitHubIssueTracker : IIssueTracker
    {
        private readonly Func<IGitHubClient> gitHubClientFactory;
        private readonly GitReleaseNotesArguments arguments;
        private readonly IRepository gitRepository;
        private readonly ILog log;

        public GitHubIssueTracker(IRepository gitRepository, Func<IGitHubClient> gitHubClientFactory, ILog log, GitReleaseNotesArguments arguments)
        {
            this.gitRepository = gitRepository;
            this.log = log;
            this.arguments = arguments;
            this.gitHubClientFactory = gitHubClientFactory;
        }

        public bool RemotePresentWhichMatches
        {
            get
            {
                return gitRepository.Network.Remotes.Any(r => r.Url.ToLower().Contains("github.com"));
            }
        }

        public string DiffUrlFormat
        {
            get
            {
                string organisation;
                string repository;
                GetRepository(arguments, out organisation, out repository);

                return "https://github.com/" + organisation + "/" + repository + "/compare/{0}...{1}";
            }
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole()
        {
            if (!RemotePresentWhichMatches)
            {
                if (arguments.Repo == null)
                {
                    log.WriteLine("GitHub repository name must be specified [/Repo .../...]");
                    return false;
                }
                var repoParts = arguments.Repo.Split('/');

                if (repoParts.Length != 2)
                {
                    log.WriteLine("GitHub repository name should be in format Organisation/RepoName");
                    return false;
                }
            }

            return true;
        }

        private void GetRepository(GitReleaseNotesArguments arguments, out string organisation, out string repository)
        {
            if (RemotePresentWhichMatches)
            {
                if (TryRemote(out organisation, out repository, "upstream"))
                    return;
                if (TryRemote(out organisation, out repository, "origin"))
                    return;
                var remoteName = gitRepository.Network.Remotes.First(r => r.Url.ToLower().Contains("github.com")).Name;
                if (TryRemote(out organisation, out repository, remoteName))
                    return;
            }

            var repoParts = arguments.Repo.Split('/');
            organisation = repoParts[0];
            repository = repoParts[1];
        }

        private bool TryRemote(out string organisation, out string repository, string remoteName)
        {
            var remote = gitRepository.Network.Remotes[remoteName];
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

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            string organisation;
            string repository;
            GetRepository(arguments, out organisation, out repository);

            var gitHubClient = gitHubClientFactory();
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
                    return user.Name;
                return null;
            };
            return readOnlyList.Select(i => new OnlineIssue
            {
                Id = "#" + i.Number.ToString(CultureInfo.InvariantCulture),
                HtmlUrl = i.HtmlUrl,
                Title = i.Title,
                IssueType = i.PullRequest == null ? IssueType.Issue : IssueType.PullRequest,
                Labels = i.Labels.Select(l => l.Name).ToArray(),
                DateClosed = i.ClosedAt.Value,
                Contributors = i.PullRequest == null ? new Contributor[0] : new[]
                {
                    new Contributor(getUserName(i.User), i.User.Login, i.User.HtmlUrl)
                }
            });
        }
    }
}