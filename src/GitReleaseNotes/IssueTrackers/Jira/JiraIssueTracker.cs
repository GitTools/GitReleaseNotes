using System;
using System.Collections.Generic;
using System.Linq;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    public class JiraIssueTracker : IIssueTracker
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly Context context;
        private readonly IJiraApi jiraApi;

        public JiraIssueTracker(IJiraApi jiraApi, Context context)
        {
            this.jiraApi = jiraApi;
            this.context = context;
        }

        public bool VerifyArgumentsAndWriteErrorsToLog()
        {
            if (string.IsNullOrEmpty(context.Jira.JiraServer) ||
                !Uri.IsWellFormedUriString(context.Jira.JiraServer, UriKind.Absolute))
            {
                Log.WriteLine("A valid Jira server must be specified [/JiraServer ]");
                return false;
            }

            if (string.IsNullOrEmpty(context.ProjectId))
            {
                Log.WriteLine("/ProjectId is a required parameter for Jira");
                return false;
            }

            if (string.IsNullOrEmpty(context.Authentication.Username))
            {
                Log.WriteLine("/Username is a required to authenticate with Jira");
                return false;
            }

            if (string.IsNullOrEmpty(context.Authentication.Password))
            {
                Log.WriteLine("/Password is a required to authenticate with Jira");
                return false;
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