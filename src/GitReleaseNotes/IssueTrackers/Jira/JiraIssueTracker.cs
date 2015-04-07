using System;
using System.Collections.Generic;
using System.Linq;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    public class JiraIssueTracker : IIssueTracker
    {
        private readonly Context context;
        private readonly IJiraApi jiraApi;
        private readonly ILog log;

        public JiraIssueTracker(IJiraApi jiraApi, ILog log, Context context)
        {
            this.jiraApi = jiraApi;
            this.log = log;
            this.context = context;
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole()
        {
            if (string.IsNullOrEmpty(context.Jira.JiraServer) ||
                !Uri.IsWellFormedUriString(context.Jira.JiraServer, UriKind.Absolute))
            {
                log.WriteLine("A valid Jira server must be specified [/JiraServer ]");
                return false;
            }

            if (string.IsNullOrEmpty(context.ProjectId))
            {
                log.WriteLine("/JiraProjectId is a required parameter for Jira");
                return false;
            }

            if (string.IsNullOrEmpty(context.Authentication.Username))
            {
                log.WriteLine("/Username is a required to authenticate with Jira");
                return false;
            }
            if (string.IsNullOrEmpty(context.Authentication.Password))
            {
                log.WriteLine("/Password is a required to authenticate with Jira");
                return false;
            }

            if (string.IsNullOrEmpty(context.Jira.Jql))
            {
                context.Jira.Jql = string.Format("project = {0} AND " +
                               "(issuetype = Bug OR issuetype = Story OR issuetype = \"New Feature\") AND " +
                               "status in (Closed, Resolved)", context.ProjectId);
            }

            return true;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            return jiraApi.GetClosedIssues(context, since).ToArray();
        }

        public bool RemotePresentWhichMatches { get { return false; }}
        public string DiffUrlFormat { get { return string.Empty; } }
    }
}