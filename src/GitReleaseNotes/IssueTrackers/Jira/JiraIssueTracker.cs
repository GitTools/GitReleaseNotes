using System;
using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.IssueTrackers.GitHub;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    public class JiraIssueTracker : IIssueTracker
    {
        private readonly IIssueNumberExtractor _issueNumberExtractor;
        private readonly IJiraApi _jiraApi;

        public JiraIssueTracker(IIssueNumberExtractor issueNumberExtractor, IJiraApi jiraApi)
        {
            _issueNumberExtractor = issueNumberExtractor;
            _jiraApi = jiraApi;
        }

        public SemanticReleaseNotes ScanCommitMessagesForReleaseNotes(
            GitReleaseNotesArguments arguments,
            Dictionary<ReleaseInfo, List<Commit>> releases)
        {
            var issueNumberRegexPattern = string.Format(@"(?<issueNumber>{0}-\d+)", arguments.JiraProjectId);
            var issueNumbersToScan = _issueNumberExtractor.GetIssueNumbers(arguments, releases, issueNumberRegexPattern);

            var potentialIssues = _jiraApi.GetPotentialIssues(releases, arguments).ToArray();

            var closedMentionedIssuesByRelease = issueNumbersToScan.Select(issues =>
            {
                var issuesForRelease = potentialIssues
                    .Where(i => issues.Value.Contains(i.Id))
                    .ToArray();
                return new
                {
                    ReleaseInfo = issues.Key,
                    IssuesForRelease = issuesForRelease
                };
            })
                .Where(g => g.IssuesForRelease.Any())
                .OrderBy(g => g.ReleaseInfo.When);

            var baseUrl = new Uri(arguments.JiraServer, UriKind.Absolute);
            return new SemanticReleaseNotes(closedMentionedIssuesByRelease.Select(r =>
            {
                var releaseNoteItems = r.IssuesForRelease.Select(i =>
                {
                    var htmlUrl = new Uri(baseUrl, string.Format("browse/{0}", i.Id));
                    return new ReleaseNoteItem(i.Name, i.Id, htmlUrl, new[] { i.IssueType });
                }).ToArray();
                return new SemanticRelease(r.ReleaseInfo.Name, r.ReleaseInfo.When, releaseNoteItems);
            }));
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole(GitReleaseNotesArguments arguments)
        {
            if (string.IsNullOrEmpty(arguments.JiraServer) ||
                !Uri.IsWellFormedUriString(arguments.JiraServer, UriKind.Absolute))
            {
                Console.WriteLine("A valid Jira server must be specified [/JiraServer ]");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.JiraProjectId))
            {
                Console.WriteLine("/JiraProjectId is a required parameter for Jira");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.Username))
            {
                Console.WriteLine("/Username is a required to authenticate with Jira");
                return false;
            }
            if (string.IsNullOrEmpty(arguments.Password))
            {
                Console.WriteLine("/Password is a required to authenticate with Jira");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.Jql))
            {
                arguments.Jql = string.Format("project = {0} AND " +
                               "(issuetype = Bug OR issuetype = Story OR issuetype = \"New Feature\") AND " +
                               "status in (Closed, Done, Resolved)", arguments.JiraProjectId);
            }

            return true;
        }

        public void PublishRelease(string releaseNotesOutput, GitReleaseNotesArguments arguments)
        {
            Console.WriteLine("Jira does not support publishing releases");
        }
    }
}