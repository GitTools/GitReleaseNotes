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

            Atlassian.Jira.Jira jira = null;

            if (!string.IsNullOrWhiteSpace(jiraContext.Token))
            {
                Log.WriteLine("Using jira with authentication token");

                jira = new Atlassian.Jira.Jira(jiraContext.Url, jiraContext.Token);
            }
            else if (!string.IsNullOrWhiteSpace(jiraContext.Username) && !string.IsNullOrWhiteSpace(jiraContext.Password))
            {
                Log.WriteLine("Using jira with username and password");

                jira = new Atlassian.Jira.Jira(jiraContext.Url, jiraContext.Username, jiraContext.Password);
            }
            else
            {
                Log.WriteLine("Using jira without authentication");

                jira = new Atlassian.Jira.Jira(jiraContext.Url);
            }

            var jql = jiraContext.Jql;
            if (string.IsNullOrEmpty(jql))
            {
                jql = string.Format("project = {0}", jiraContext.ProjectId);

                try
                {
                    var issueTypes = jira.GetIssueTypes(jiraContext.ProjectId);
                    jql += string.Format(" AND issuetype in ({0})", string.Join(", ", issueTypes.Select(x => string.Format("\"{0}\"", x.Name))));
                }
                catch (Exception ex)
                {
                    Log.WriteLine("Failed to retrieve issue types, defaulting to all issue types");
                }

                try
                {
                    var issueStatuses = jira.GetIssueStatuses();
                    jql += string.Format(" AND status in ({0})", string.Join(", ", issueStatuses.Where(x => _knownIssueStatuses.Contains(x.Name.ToLower())).Select(x => string.Format("\"{0}\"", x.Name))));
                }
                catch (Exception ex)
                {
                    Log.WriteLine("Failed to retrieve issue statuses, defaulting to issue statuses issue 'Closed'");

                    jql += " AND status in (Closed)";
                }
            }

            if (since.HasValue)
            {
                var sinceFormatted = since.Value.ToString("yyyy-MM-d HH:mm");
                jql += string.Format(" AND updated > '{0}'", sinceFormatted).Replace("\"", "\\\"");
            }

            // Update back so every component is aware of the new jql
            jiraContext.Jql = jql;

            var issues = jira.GetIssuesFromJql(jql, 200);
            foreach (var issue in issues)
            {
                var summary = issue.Summary;
                var id = issue.Key.Value;
                //var issueType = issue.Type.Name;

                var closedDate = issue.Created ?? DateTime.Today;

                try
                {
                    closedDate = issue.GetResolutionDate() ?? DateTime.Today;
                }
                catch (Exception)
                {
                    Log.WriteLine("Failed to retrieve the resolution date of '{0}', falling back to creation date", id);
                }

                yield return new OnlineIssue(id, new DateTimeOffset(closedDate))
                {
                    Title = summary,
                    IssueType = IssueType.Issue,
                    HtmlUrl = new Uri(new Uri(jiraContext.Url), string.Format("browse/{0}", id))
                };
            }
        }
    }
}