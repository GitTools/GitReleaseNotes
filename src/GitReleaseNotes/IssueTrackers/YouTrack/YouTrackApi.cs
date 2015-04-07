using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace GitReleaseNotes.IssueTrackers.YouTrack
{
    public sealed class YouTrackApi : IYouTrackApi
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private static CookieCollection ConnectToYouTrack(string userName, string password, string youtrackHostUrl)
        {
            var loginUrl = string.Format(
                CultureInfo.InvariantCulture,
                "{0}/user/login",
                youtrackHostUrl);

            string loginText =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "login={0}&password={1}",
                        userName,
                        password);
            var loginBytes = Encoding.ASCII.GetBytes(loginText);

            var httpRequest = WebRequest.CreateHttp(loginUrl);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.CookieContainer = new CookieContainer();
            httpRequest.ContentLength = loginBytes.Length;

            var stream = httpRequest.GetRequestStream();
            stream.Write(loginBytes, 0, loginBytes.Length);

            var response = (HttpWebResponse)httpRequest.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to log in with YouTrack: " + response.StatusDescription);
            }

            var result = new CookieCollection
            {
                response.Cookies
            };
            return result;
        }

        private static IEnumerable<OnlineIssue> IssuesClosedSinceDate(
            CookieCollection authenticationCookies,
            string filter,
            string youtrackHostUrl,
            string projectId,
            DateTimeOffset? since)
        {
            const int maxPerRequest = 5;
            const string issueByDateRequestTemplate = "{0}/rest/issue/byproject/{1}?filter={2}&after={3}&max={4}";
            var result = new List<OnlineIssue>();

            string query;
            if (since.HasValue)
            {
                query = string.Format(
                    "{0} resolved date: {1:yyyy-MM-ddTHH:mm:ss} .. {2:yyyy-MM-ddTHH:mm:ss}",
                    filter,
                    since.Value,
                    DateTimeOffset.Now);
            }
            else
            {
                query = filter;
            }

            int startAt = 0;
            bool shouldRequestMoreIssues = true;
            while (shouldRequestMoreIssues)
            {
                var filterText = HttpUtility.UrlEncode(query);
                var issueByDateRequest = string.Format(
                    CultureInfo.InvariantCulture,
                    issueByDateRequestTemplate,
                    youtrackHostUrl,
                    projectId,
                    filterText,
                    startAt,
                    maxPerRequest);

                var httpRequest = WebRequest.CreateHttp(issueByDateRequest);
                httpRequest.Method = "GET";
                httpRequest.ContentType = "application/x-www-form-urlencoded";
                httpRequest.CookieContainer = new CookieContainer();
                httpRequest.CookieContainer.Add(authenticationCookies);

                var response = (HttpWebResponse)httpRequest.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Failed to get issues from YouTrack: " + response.StatusDescription);
                }

                string rawText;
                using (var responseStream = response.GetResponseStream())
                {
                    using (var responseReader = new StreamReader(responseStream))
                    {
                        rawText = responseReader.ReadToEnd();
                    }
                }

                var doc = XDocument.Parse(rawText, LoadOptions.None);
                var issues = from element in doc.Elements("issues").Descendants("issue")
                             select new
                             {
                                 Id = element.Attribute("id").Value,
                                 Type = (from subElement in element.Descendants("field")
                                         where string.Equals("Type", subElement.Attribute("name").Value, StringComparison.OrdinalIgnoreCase)
                                         select subElement.Element("value").Value).FirstOrDefault(),
                                 Summary = (from subElement in element.Descendants("field")
                                            where string.Equals("summary", subElement.Attribute("name").Value, StringComparison.OrdinalIgnoreCase)
                                            select subElement.Element("value").Value).FirstOrDefault(),
                                 Resolved = (from subElement in element.Descendants("field")
                                             where string.Equals("resolved", subElement.Attribute("name").Value, StringComparison.InvariantCultureIgnoreCase)
                                             select subElement.Element("value").Value).FirstOrDefault(),
                             };

                int count = 0;
                foreach (var issue in issues)
                {
                    long milliSecondsSinceEpochStart;
                    if (!long.TryParse(issue.Resolved, out milliSecondsSinceEpochStart))
                    {
                        milliSecondsSinceEpochStart = 0L;
                    }

                    var epochTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero) + TimeSpan.FromMilliseconds(milliSecondsSinceEpochStart);
                    result.Add(
                        new OnlineIssue(issue.Id, epochTime)
                        {
                            Title = issue.Summary,
                            IssueType = IssueType.Issue,
                            HtmlUrl = new Uri(new Uri(youtrackHostUrl, UriKind.Absolute), string.Format("issue/{0}", issue.Id))
                        });

                    count++;
                }

                shouldRequestMoreIssues = false;
                if (count == maxPerRequest)
                {
                    startAt += maxPerRequest;
                    shouldRequestMoreIssues = true;
                }
            }

            return result;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(Context context, DateTimeOffset? since)
        {
            var authenticationCookies = ConnectToYouTrack(context.Authentication.Username, context.Authentication.Password, context.YouTrack.YouTrackServer);
            return IssuesClosedSinceDate(
                authenticationCookies,
                context.YouTrack.YouTrackFilter,
                context.YouTrack.YouTrackServer,
                context.ProjectId,
                since);
        }
    }
}
