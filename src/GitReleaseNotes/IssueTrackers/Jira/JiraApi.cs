using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using RestSharp;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    public class JiraApi : IJiraApi
    {
        public IEnumerable<OnlineIssue> GetClosedIssues(Context context, DateTimeOffset? since)
        {
            var jira = new Atlassian.Jira.Jira(context.Jira.JiraServer, context.Authentication.Username, context.Authentication.Password);

            var jql = context.Jira.Jql;
            if (string.IsNullOrEmpty(jql))
            {
                jql = string.Format("project = {0}", context.ProjectId);

                var issueTypes = jira.GetIssueTypes(context.ProjectId);
                jql += string.Format(" AND issuetype in ({0})", string.Join(", ", issueTypes.Select(x => string.Format("\"{0}\"", x.Name))));

                var knownIssueStatuses = new HashSet<string>(new[] { "closed", "resolved", "done" });

                var issueStatuses = jira.GetIssueStatuses();
                jql += string.Format(" AND status in ({0})", string.Join(", ", issueStatuses.Where(x => knownIssueStatuses.Contains(x.Name.ToLower())).Select(x => string.Format("\"{0}\"", x.Name))));
            }

            if (since.HasValue)
            {
                var sinceFormatted = since.Value.ToString("yyyy-MM-d HH:mm");
                jql += string.Format(" AND updated > '{0}'", sinceFormatted).Replace("\"", "\\\"");
            }

            // Update back so every component is aware of the new jql
            context.Jira.Jql = jql;

            var issues = jira.GetIssuesFromJql(jql);
            foreach (var issue in issues)
            {
                var summary = issue.Summary;
                var id = issue.Key.Value;
                var issueType = issue.Type.Name;

                yield return new OnlineIssue
                {
                    Id = id,
                    Title = summary,
                    IssueType = IssueType.Issue,
                    HtmlUrl = new Uri(new Uri(context.Jira.JiraServer), string.Format("browse/{0}", id))
                };
            }
        }
    }
}