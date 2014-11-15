using System;
using System.Collections.Generic;
using System.Linq;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    public class JiraIssueTracker : IIssueTracker
    {
        private readonly GitReleaseNotesArguments arguments;
        private readonly IJiraApi jiraApi;
        private readonly ILog log;

        public JiraIssueTracker(IJiraApi jiraApi, ILog log, GitReleaseNotesArguments arguments)
        {
            this.jiraApi = jiraApi;
            this.log = log;
            this.arguments = arguments;
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole()
        {
            if (string.IsNullOrEmpty(arguments.JiraServer) ||
                !Uri.IsWellFormedUriString(arguments.JiraServer, UriKind.Absolute))
            {
                log.WriteLine("A valid Jira server must be specified [/JiraServer ]");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.ProjectId))
            {
                log.WriteLine("/JiraProjectId is a required parameter for Jira");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.Username))
            {
                log.WriteLine("/Username is a required to authenticate with Jira");
                return false;
            }
            if (string.IsNullOrEmpty(arguments.Password))
            {
                log.WriteLine("/Password is a required to authenticate with Jira");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.Jql))
            {
                arguments.Jql = string.Format("project = {0} AND " +
                               "(issuetype = Bug OR issuetype = Story OR issuetype = \"New Feature\") AND " +
                               "status in (Closed, Resolved)", arguments.ProjectId);
            }

            return true;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            return jiraApi.GetClosedIssues(arguments, since).ToArray();
        }

        public bool RemotePresentWhichMatches { get { return false; }}
        public string DiffUrlFormat { get { return string.Empty; } }
    }
}