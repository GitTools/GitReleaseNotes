using System.ComponentModel;
using GitReleaseNotes.IssueTrackers;

namespace GitReleaseNotes
{
    public class GitReleaseNotesArguments
    {
        [Description("The directory of the Git repository to determine the version for; " +
                     "if unspecified it will search parent directories recursively until finding a Git repository.")]
        public string WorkingDirectory { get; set; }

        [Description("Generates release notes based on issues closed since date of specified tag")]
        public bool FromClosedIssues { get; set; }

        [Description("Generates release notes from issues mentioned in commit messages")]
        public bool FromMentionedIssues { get; set; }

        [Description("Enables verbose logging")]
        public bool Verbose { get; set; }

        [Description("Specifies the issue tracker used, possible Options: GitHub, Jira")]
        public IssueTracker? IssueTracker { get; set; }

        [Description("Specify the tag name to start from, default is the last tag on master")]
        public string FromTag { get; set; }

        [Description("GitHub access token")]
        public string Token { get; set; }

        [Description("Issue tracker username")]
        public string Username { get; set; }

        [Description("Issue tracker password")]
        public string Password { get; set; }

        [Description("Jira project ID")]
        public string JiraProjectId { get; set; }

        [Description("Jql query for closed issues you would like included if mentioned. Defaults to project = <YOURPROJECTID> AND (issuetype = Bug OR issuetype = Story OR issuetype = \"New Feature\") AND status in (Closed, Done, Resolved)")]
        public string Jql { get; set; }

        [Description("Url of Jira server")]
        public string JiraServer { get; set; }

        [Description("Url of YouTrack server")]
        public string YouTrackServer { get; set; }

        [Description("YouTrack project ID")]
        public string YouTrackProjectId { get; set; }

        [Description("YouTrack filter for closed issues that you would like included if mentioned.")]
        public string YouTrackFilter { get; set; }

        [Description("GitHub Repository name, in Organisation/Repository format")]
        public string Repo { get; set; }

        [Description("The release notes file")]
        public string OutputFile { get; set; }

        [Description("Allows additional labels to be treated as categories")]
        public string Categories { get; set; }

        [Description("Publish the release to the specified issue tracker")]
        public bool Publish { get; set; }

        [Description("Specifies the version to publish")]
        public string Version { get; set; }
    }
}