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
        private readonly IGitHubClient _gitHubClient;

        public GitHubIssueTracker(IIssueNumberExtractor issueNumberExtractor, IGitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient;
            _issueNumberExtractor = issueNumberExtractor;
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole(GitReleaseNotesArguments arguments)
        {
            if (arguments.Repo == null)
            {
                Console.WriteLine("GitHub repository cannot be null");
                return false;
            }
            var repoParts = arguments.Repo.Split('/');

            if (repoParts.Length != 2)
            {
                Console.WriteLine("GitHub repository name should be in format Organisation/RepoName");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.Token))
            {
                Console.WriteLine("You must specify a GitHub Authentication token with the /Token argument");
                return false;
            }

            return true;
        }

        public SemanticReleaseNotes ScanCommitMessagesForReleaseNotes(GitReleaseNotesArguments arguments, Dictionary<ReleaseInfo, List<Commit>> releases)
        {
            var repoParts = arguments.Repo.Split('/');
            var organisation = repoParts[0];
            var repository = repoParts[1];

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

        private IEnumerable<Issue> GetPotentialIssues(Dictionary<ReleaseInfo, List<Commit>> releases, string organisation, string repository)
        {
            var since = releases.SelectMany(c => c.Value).Select(c => c.Author.When).Min();
            return _gitHubClient.Issue.GetForRepository(organisation, repository, new RepositoryIssueRequest
            {
                Filter = IssueFilter.All,
                Since = since,
                State = ItemState.Closed
            }).Result;
        }
    }
}