using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using RestSharp;

namespace GitReleaseNotes.IssueTrackers.BitBucket
{
    public class BitBucketApi : IBitBucketApi
    {
        private const string IssueClosed = "resolved";
        private const string ApiUrl = "https://bitbucket.org/api/1.0/";

        public IEnumerable<OnlineIssue> GetClosedIssues(GitReleaseNotesArguments arguments, DateTimeOffset? since, string accountName, string repoSlug, bool oauth)
        {
            var baseUrl = new Uri(ApiUrl, UriKind.Absolute);
            var restClient = new RestClient(baseUrl.AbsoluteUri);
            var issuesUrl = string.Format("repositories/{0}/{1}/issues/", accountName, repoSlug);
            var request = new RestRequest(issuesUrl);
            if (oauth)
            {
                GenerateOauthRequest(arguments, baseUrl, issuesUrl, request);
            }
            else
            {
                GenerateClassicalRequest(arguments, request, issuesUrl);
            }
            var response = restClient.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to query BitBucket: " + response.StatusDescription);
            }
            dynamic responseObject = SimpleJson.DeserializeObject(response.Content);
            var issues = new List<OnlineIssue>();
            foreach (var issue in responseObject.issues)
            {
                DateTimeOffset lastChange = DateTimeOffset.Parse(issue.utc_last_updated);
                if (issue.status != IssueClosed || lastChange <= since)
                {
                    continue;
                }
                string summary = issue.content;
                string id = issue.local_id.ToString();
                string title = issue.title;
                issues.Add(new OnlineIssue
                {
                    Id = id,
                    Title = summary,
                    IssueType = IssueType.Issue,
                    HtmlUrl = new Uri(baseUrl, string.Format("/repositories/{0}/{1}/issue/{2}/{3}", accountName, repoSlug, id, title)),
                    DateClosed = lastChange
                });
            }
            return issues;
        }

        private static void GenerateClassicalRequest(GitReleaseNotesArguments arguments, RestRequest request, string MethodLocation)
        {
            var usernameAndPass = string.Format("{0}:{1}", arguments.Username, arguments.Password);
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameAndPass));
            request.Resource = string.Format(MethodLocation);
            request.AddHeader("Authorization", string.Format("Basic {0}", token));
        }

        private static void GenerateOauthRequest(GitReleaseNotesArguments arguments, Uri baseUrl, string MethodLocation, RestRequest request)
        {
            var consumerKey = arguments.Username;
            var consumerSecret = arguments.Password;
            var oAuth = new OAuthBase();
            var nonce = oAuth.GenerateNonce();
            var timeStamp = oAuth.GenerateTimeStamp();
            string normalizedUrl;
            string normalizedRequestParameters;
            var sig = oAuth.GenerateSignature(new Uri(baseUrl + MethodLocation), consumerKey, consumerSecret, null, null, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);

            request.Resource = string.Format(MethodLocation);
            request.Method = Method.GET;
            request.AddParameter("oauth_consumer_key", consumerKey);
            request.AddParameter("oauth_nonce", nonce);
            request.AddParameter("oauth_timestamp", timeStamp);
            request.AddParameter("oauth_signature_method", "HMAC-SHA1");
            request.AddParameter("oauth_version", "1.0");
            request.AddParameter("oauth_signature", sig);
        }
    }
}