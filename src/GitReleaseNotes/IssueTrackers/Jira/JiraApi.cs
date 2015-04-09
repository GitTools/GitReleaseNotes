namespace GitReleaseNotes.IssueTrackers.Jira
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class JiraApi : IJiraApi
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly HashSet<string> _knownIssueStatuses = new HashSet<string>(new[] { "closed", "resolved", "done" });
        //private readonly Dictionary<string, IssueType> _issueTypeMappings = new Dictionary<string, IssueType>();

        public JiraApi()
        {
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(IIssueTrackerContext context, DateTimeOffset? since)
        {
            var jiraContext = (JiraContext) context;

            var jira = new Atlassian.Jira.Jira(jiraContext.Url, jiraContext.Username, jiraContext.Password);

            var jql = jiraContext.Jql;
            if (string.IsNullOrEmpty(jql))
            {
                jql = string.Format("project = {0}", jiraContext.ProjectId);

                var issueTypes = jira.GetIssueTypes(jiraContext.ProjectId);
                jql += string.Format(" AND issuetype in ({0})", string.Join(", ", issueTypes.Select(x => string.Format("\"{0}\"", x.Name))));

                var issueStatuses = jira.GetIssueStatuses();
                jql += string.Format(" AND status in ({0})", string.Join(", ", issueStatuses.Where(x => _knownIssueStatuses.Contains(x.Name.ToLower())).Select(x => string.Format("\"{0}\"", x.Name))));
            }

            if (since.HasValue)
            {
                var sinceFormatted = since.Value.ToString("yyyy-MM-d HH:mm");
                jql += string.Format(" AND updated > '{0}'", sinceFormatted).Replace("\"", "\\\"");
            }

            // Update back so every component is aware of the new jql
            jiraContext.Jql = jql;

            var issues = jira.GetIssuesFromJql(jql);
            foreach (var issue in issues)
            {
                var summary = issue.Summary;
                var id = issue.Key.Value;
                //var issueType = issue.Type.Name;

                yield return new OnlineIssue(id, issue.GetResolutionDate().Value)
                {
                    Title = summary,
                    IssueType = IssueType.Issue,
                    HtmlUrl = new Uri(new Uri(jiraContext.Url), string.Format("browse/{0}", id))
                };
            }
        }
    }
}