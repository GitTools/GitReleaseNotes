using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using RestSharp;

namespace GitReleaseNotes.IssueTrackers.BitBucket
{
    using Newtonsoft.Json;

    public class BitBucketApi : IBitBucketApi
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private const string IssueClosed = "closed";
        private const string IssueResolved = "resolved";
        private const string ApiUrl = "https://bitbucket.org/api/1.0/";

        public IEnumerable<OnlineIssue> GetClosedIssues(Context context, DateTimeOffset? since, string accountName, string repoSlug, bool oauth)
        {
            var baseUrl = new Uri(ApiUrl, UriKind.Absolute);
            var restClient = new RestClient(baseUrl.AbsoluteUri);
            var issuesUrl = string.Format("repositories/{0}/{1}/issues/", accountName, repoSlug);
            var request = new RestRequest(issuesUrl);
            if (oauth)
            {
                GenerateOauthRequest(context, baseUrl, issuesUrl, request);
            }
            else
            {
                GenerateClassicalRequest(context, request, issuesUrl);
            }

            var response = restClient.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to query BitBucket: " + response.StatusDescription);
            }

            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
            
            var issues = new List<OnlineIssue>();
            foreach (var issue in responseObject.issues)
            {
                DateTimeOffset lastChange = DateTimeOffset.Parse(issue.utc_last_updated.ToString());
                if ((issue.status != IssueClosed && issue.status != IssueResolved) || lastChange <= since)
                {
                    continue;
                }
                string summary = issue.content;
                string id = issue.local_id.ToString();
                string title = issue.title;
                issues.Add(new OnlineIssue(id, lastChange)
                {
                    Title = summary,
                    IssueType = IssueType.Issue,
                    HtmlUrl = new Uri(baseUrl, string.Format("/repositories/{0}/{1}/issue/{2}/{3}", accountName, repoSlug, id, title))
                });
            }
            return issues;
        }

        private static void GenerateClassicalRequest(Context context, RestRequest request, string methodLocation)
        {
            var usernameAndPass = string.Format("{0}:{1}", context.Authentication.Username, context.Authentication.Password);
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameAndPass));
            request.Resource = string.Format(methodLocation);
            request.AddHeader("Authorization", string.Format("Basic {0}", token));
        }

        private static void GenerateOauthRequest(Context context, Uri baseUrl, string methodLocation, RestRequest request)
        {
            var consumerKey = string.IsNullOrEmpty(context.Authentication.Username) ? context.BitBucket.ConsumerKey : context.Authentication.Username;
            var consumerSecret = string.IsNullOrEmpty(context.Authentication.Password) ? context.BitBucket.ConsumerSecretKey : context.Authentication.Password;
            var oAuth = new OAuthBase();
            var nonce = oAuth.GenerateNonce();
            var timeStamp = oAuth.GenerateTimeStamp();
            string normalizedUrl;
            string normalizedRequestParameters;
            var sig = oAuth.GenerateSignature(new Uri(baseUrl + methodLocation), consumerKey, consumerSecret, null, null, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);

            request.Resource = string.Format(methodLocation);
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