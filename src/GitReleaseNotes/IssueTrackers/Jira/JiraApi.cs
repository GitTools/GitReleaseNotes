using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Helpers;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    public interface IJiraApi
    {
        IEnumerable<JiraIssue> GetPotentialIssues(Dictionary<ReleaseInfo, List<Commit>> releases, GitReleaseNotesArguments arguments);
    }

    public class JiraApi : IJiraApi
    {
        public IEnumerable<JiraIssue> GetPotentialIssues(Dictionary<ReleaseInfo, List<Commit>> releases, GitReleaseNotesArguments arguments)
        {
            var since = releases.SelectMany(c => c.Value).Select(c => c.Author.When).Min();
            var sinceFormatted = since.ToString("yyyy-MM-d HH:mm");
            var jql = string.Format("{0} AND updated > '{1}'", arguments.Jql, sinceFormatted).Replace("\"", "\\\"");

            var searchUri = new Uri(new Uri(arguments.JiraServer, UriKind.Absolute), "/rest/api/latest/search");
            var httpRequest = WebRequest.CreateHttp(searchUri);
            var usernameAndPass = string.Format("{0}:{1}", arguments.Username, arguments.Password);
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameAndPass));
            httpRequest.Headers.Add("Authorization", string.Format("Basic {0}", token));
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                string json = "{\"jql\": \"" + jql + "\",\"startAt\": 0, \"maxResults\": 100, \"fields\": [\"summary\",\"issuetype\"]}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var response = (HttpWebResponse)httpRequest.GetResponse();
            if ((int)response.StatusCode == 400)
            {
                throw new Exception("Jql query error, please review your Jql");
            }

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Failed to query Jira: " + response.StatusDescription);

            using (var responseStream = response.GetResponseStream())
            using (var responseReader = new StreamReader(responseStream))
            {
                var responseObject = Json.Decode(responseReader.ReadToEnd());
                foreach (var issue in responseObject.issues)
                {
                    string summary = issue.fields.summary;
                    string id = issue.key;
                    string issueType = issue.fields.issuetype.name;

                    yield return new JiraIssue
                    {
                        Id = id,
                        Name = summary,
                        IssueType = issueType
                    };
                }
            }
        }
    }
}