using System.ComponentModel;
using GitReleaseNotes.IssueTrackers;

namespace GitReleaseNotes
{
    public class GitReleaseNotesArguments
    {
        [Description("The directory of the Git repository to determine the version for; " +
                     "if unspecified it will search parent directories recursively until finding a Git repository.")]
        public string WorkingDirectory { get; set; }

        [Description("Enables verbose logging")]
        public bool Verbose { get; set; }

        [Description("Specifies the issue tracker used, possible Options: GitHub, Jira")]
        public IssueTracker? IssueTracker { get; set; }

        [Description("Speficy the tag name to start from, default is the last tag on master")]
        public string FromTag { get; set; }

        [Description("GitHub access token")]
        public string Token { get; set; }

        [Description("Jira Username")]
        public string Username { get; set; }

        [Description("Jira Password")]
        public string Password { get; set; }

        [Description("Jira project Id")]
        public string JiraProjectId { get; set; }

        [Description("Jql query for closed issues you would like included if mentioned. Defaults to project = <YOURPROJECTID> AND (issuetype = Bug OR issuetype = Story OR issuetype = \"New Feature\") AND status in (Closed, Done, Resolved)")]
        public string Jql { get; set; }

        [Description("Url of Jira server")]
        public string JiraServer { get; set; }

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