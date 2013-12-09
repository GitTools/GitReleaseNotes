using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibGit2Sharp;
using Octokit;

namespace GitReleaseNotes.IssueTrackers.GitHub
{
    // ReSharper disable NotResolvedInText
    public class GitHubIssueTracker : IIssueTracker
    {
        private readonly IIssueNumberExtractor _issueNumberExtractor;
        private readonly Func<IGitHubClient> _gitHubClientFactory;
        private readonly ILog _log;

        public GitHubIssueTracker(IIssueNumberExtractor issueNumberExtractor, Func<IGitHubClient> gitHubClientFactory, ILog log)
        {
            _log = log;
            _issueNumberExtractor = issueNumberExtractor;
            _gitHubClientFactory = gitHubClientFactory;
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole(GitReleaseNotesArguments arguments)
        {
            if (arguments.Repo == null)
            {
                _log.WriteLine("GitHub repository name must be specified [/Repo .../...]");
                return false;
            }
            var repoParts = arguments.Repo.Split('/');

            if (repoParts.Length != 2)
            {
                _log.WriteLine("GitHub repository name should be in format Organisation/RepoName");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.Token))
            {
                _log.WriteLine("You must specify a GitHub Authentication token with the /Token argument");
                return false;
            }

            if (arguments.Publish && string.IsNullOrEmpty(arguments.Version))
            {
                _log.WriteLine("You must specifiy the version [/Version ...] (will be tag) when using the /Publish flag");
                return false;
            }

            return true;
        }

        public void PublishRelease(string releaseNotesOutput, GitReleaseNotesArguments arguments)
        {
            string organisation;
            string repository;
            GetRepository(arguments, out organisation, out repository);

            var releaseUpdate = new ReleaseUpdate(arguments.Version)
            {
                Name = arguments.Version,
                Body = releaseNotesOutput
            };
            var release = _gitHubClientFactory().Release.CreateRelease(organisation, repository, releaseUpdate);
            release.Wait();
        }

        public SemanticReleaseNotes ScanCommitMessagesForReleaseNotes(GitReleaseNotesArguments arguments, Dictionary<ReleaseInfo, List<Commit>> releases)
        {
            string organisation;
            string repository;
            GetRepository(arguments, out organisation, out repository);

            var issueNumbersToScan = _issueNumberExtractor.GetIssueNumbers(arguments, releases, @"#(?<issueNumber>\d+)");

            var potentialIssues = GetPotentialIssues(releases, organisation, repository).ToArray();

            var closedMentionedIssuesByRelease = issueNumbersToScan.Select(issues =>
            {
                var issuesForRelease = potentialIssues
                    .Where(i => issues.Value.Contains(i.Number.ToString(CultureInfo.InvariantCulture)))
                    .ToArray();
                return new
                {
                    ReleaseInfo = issues.Key,
                    IssuesForRelease = issuesForRelease
                };
            })
                .Where(g => g.IssuesForRelease.Any())
                .OrderBy(g=>g.ReleaseInfo.When);

            return new SemanticReleaseNotes(closedMentionedIssuesByRelease.Select(r =>
            {
                var releaseNoteItems = r.IssuesForRelease.Select(i =>
                {
                    var labels = i.Labels == null ? new string[0] : i.Labels.Select(l => l.Name).ToArray();
                    return new ReleaseNoteItem(i.Title, string.Format("#{0}", i.Number), i.HtmlUrl, labels);
                }).ToArray();
                return new SemanticRelease(r.ReleaseInfo.Name, r.ReleaseInfo.When, releaseNoteItems);
                
            }));
        }

        private static void GetRepository(GitReleaseNotesArguments arguments, out string organisation, out string repository)
        {
            var repoParts = arguments.Repo.Split('/');
            organisation = repoParts[0];
            repository = repoParts[1];
        }

        private IEnumerable<Issue> GetPotentialIssues(Dictionary<ReleaseInfo, List<Commit>> releases, string organisation, string repository)
        {
            var since = releases.SelectMany(c => c.Value).Select(c => c.Author.When).Min();
            return _gitHubClientFactory().Issue.GetForRepository(organisation, repository, new RepositoryIssueRequest
            {
                Filter = IssueFilter.All,
                Since = since,
                State = ItemState.Closed
            }).Result;
        }
    }
}