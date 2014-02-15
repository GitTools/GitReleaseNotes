using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    public class JiraIssueTracker : IIssueTracker
    {
        private readonly GitReleaseNotesArguments _arguments;
        private readonly Regex _issueNumberRegex;
        private readonly IJiraApi _jiraApi;

        public JiraIssueTracker(IJiraApi jiraApi, GitReleaseNotesArguments arguments)
        {
            _jiraApi = jiraApi;
            _arguments = arguments;
            _issueNumberRegex = new Regex(string.Format(@"(?<issueNumber>{0}-\d+)", arguments.JiraProjectId));
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole()
        {
            if (string.IsNullOrEmpty(_arguments.JiraServer) ||
                !Uri.IsWellFormedUriString(_arguments.JiraServer, UriKind.Absolute))
            {
                Console.WriteLine("A valid Jira server must be specified [/JiraServer ]");
                return false;
            }

            if (string.IsNullOrEmpty(_arguments.JiraProjectId))
            {
                Console.WriteLine("/JiraProjectId is a required parameter for Jira");
                return false;
            }

            if (string.IsNullOrEmpty(_arguments.Username))
            {
                Console.WriteLine("/Username is a required to authenticate with Jira");
                return false;
            }
            if (string.IsNullOrEmpty(_arguments.Password))
            {
                Console.WriteLine("/Password is a required to authenticate with Jira");
                return false;
            }

            if (string.IsNullOrEmpty(_arguments.Jql))
            {
                _arguments.Jql = string.Format("project = {0} AND " +
                               "(issuetype = Bug OR issuetype = Story OR issuetype = \"New Feature\") AND " +
                               "status in (Closed, Done, Resolved)", _arguments.JiraProjectId);
            }

            return true;
        }

        public void PublishRelease(string releaseNotesOutput)
        {
            Console.WriteLine("Jira does not support publishing releases");
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            return _jiraApi.GetClosedIssues(_arguments, since).ToArray();
        }

        public Regex IssueNumberRegex
        {
            get { return _issueNumberRegex; }
        }
    }
}