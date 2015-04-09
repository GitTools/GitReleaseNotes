using System;
using System.Collections.Generic;
using System.Linq;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    public class JiraIssueTracker : IIssueTracker
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly Context _context;
        private readonly IJiraApi _jiraApi;

        public JiraIssueTracker(IJiraApi jiraApi, Context context)
        {
            _jiraApi = jiraApi;
            _context = context;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(IIssueTrackerContext context, DateTimeOffset? since)
        {
            return _jiraApi.GetClosedIssues(context, since).ToArray();
        }
    }
}