using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Octokit;

namespace GitReleaseNotes.IssueTrackers.GitHub
{
    public class GitHubIssueTracker : IIssueTracker
    {
        private readonly Func<IGitHubClient> _gitHubClientFactory;
        private readonly ILog _log;
        private readonly GitReleaseNotesArguments _arguments;

        public GitHubIssueTracker(Func<IGitHubClient> gitHubClientFactory, ILog log, GitReleaseNotesArguments arguments)
        {
            _log = log;
            _arguments = arguments;
            _gitHubClientFactory = gitHubClientFactory;
            IssueNumberRegex = new Regex(@"#(?<issueNumber>\d+)", RegexOptions.Compiled);
        }

        public Regex IssueNumberRegex { get; private set; }

        public bool VerifyArgumentsAndWriteErrorsToConsole()
        {
            if (_arguments.Repo == null)
            {
                _log.WriteLine("GitHub repository name must be specified [/Repo .../...]");
                return false;
            }
            var repoParts = _arguments.Repo.Split('/');

            if (repoParts.Length != 2)
            {
                _log.WriteLine("GitHub repository name should be in format Organisation/RepoName");
                return false;
            }

            if (string.IsNullOrEmpty(_arguments.Token))
            {
                _log.WriteLine("You must specify a GitHub Authentication token with the /Token argument");
                return false;
            }

            if (_arguments.Publish && string.IsNullOrEmpty(_arguments.Version))
            {
                _log.WriteLine("You must specifiy the version [/Version ...] (will be tag) when using the /Publish flag");
                return false;
            }

            return true;
        }

        public void PublishRelease(string releaseNotesOutput)
        {
            string organisation;
            string repository;
            GetRepository(_arguments, out organisation, out repository);

            var releaseUpdate = new ReleaseUpdate(_arguments.Version)
            {
                Name = _arguments.Version,
                Body = releaseNotesOutput
            };
            var release = _gitHubClientFactory().Release.CreateRelease(organisation, repository, releaseUpdate);
            release.Wait();
        }

        private static void GetRepository(GitReleaseNotesArguments arguments, out string organisation, out string repository)
        {
            var repoParts = arguments.Repo.Split('/');
            organisation = repoParts[0];
            repository = repoParts[1];
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            string organisation;
            string repository;
            GetRepository(_arguments, out organisation, out repository);

            var forRepository = _gitHubClientFactory().Issue.GetForRepository(organisation, repository, new RepositoryIssueRequest
            {
                Filter = IssueFilter.All,
                Since = since,
                State = ItemState.Closed
            });
            var readOnlyList = forRepository.Result;
            return readOnlyList.Select(i=>new OnlineIssue
            {
                HtmlUrl = i.HtmlUrl,
                Id = i.Number.ToString(CultureInfo.InvariantCulture),
                IssueType = i.PullRequest == null ? IssueType.Issue : IssueType.PullRequest,
                Labels = i.Labels.Select(l=>l.Name).ToArray(),
                Title = i.Title,
                DateClosed = i.ClosedAt.Value
            });
        }
    }
}