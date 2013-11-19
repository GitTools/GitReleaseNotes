using System;
using System.Linq;
using System.Net.Http.Headers;
using LibGit2Sharp;
using Octokit;

namespace GitReleaseNotes.IssueTrackers.GitHub
{
    // ReSharper disable NotResolvedInText
    public class GitHubIssueTracker : IIssueTracker
    {
        private readonly IIssueNumberExtractor _issueNumberExtractor;

        public GitHubIssueTracker(IIssueNumberExtractor issueNumberExtractor)
        {
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

        public SemanticReleaseNotes ScanCommitMessagesForReleaseNotes(GitReleaseNotesArguments arguments, Commit[] commitsToScan)
        {
            var repoParts = arguments.Repo.Split('/');
            var organisation = repoParts[0];
            var repository = repoParts[1];

            var issueNumbersToScan = _issueNumberExtractor.GetIssueNumbers(arguments, commitsToScan, @"#(?<issueNumber>\d+)");

            var github = new GitHubClient(new ProductHeaderValue("OctokitTests"))
            {
                Credentials = new Octokit.Credentials(arguments.Token)
            };

            var since = commitsToScan.Select(c=>c.Author.When).Min();
            var potentialIssues = github.Issue.GetForRepository(organisation, repository, new RepositoryIssueRequest
            {
                Filter = IssueFilter.All,
                Since = since,
                State = ItemState.Closed
            }).Result;

            var closedMentionedIssues = potentialIssues
                .Where(i => issueNumbersToScan.Contains(i.Number.ToString()))
                .ToArray();
            
            return new SemanticReleaseNotes(closedMentionedIssues.Select(i=>
                new ReleaseNoteItem(i.Title, string.Format("#{0}", i.Number), i.HtmlUrl, i.Labels.Select(l=>l.Name).ToArray())));
        }
    }
}