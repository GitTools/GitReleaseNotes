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

        [Description("Specifies the issue tracker used, possible Options: GitHub, Jira, YouTrack, BitBucket")]
        public IssueTracker? IssueTracker { get; set; }

        [Description("Specifies that all tags should be included in the release notes, if not specified then only the issues since the last tag are included.")]
        public bool AllTags { get; set; }

        [Description("GitHub access token")]
        public string Token { get; set; }

        [Description("GitHub username")]
        public string RepoUsername { get; set; }

        [Description("GitHub password")]
        public string RepoPassword { get; set; }

        [Description("Url of repository")]
        public string RepoUrl { get; set; }

        [Description("The branch name to checkout any existing release notes file")]
        public string RepoBranch { get; set; }

        [Description("Issue tracker username")]
        public string Username { get; set; }

        [Description("Issue tracker password")]
        public string Password { get; set; }

        [Description("Issue tracker project ID")]
        public string ProjectId { get; set; }

        [Description("Jql query for closed issues you would like included if mentioned. Defaults to project = <YOURPROJECTID> AND (issuetype = Bug OR issuetype = Story OR issuetype = \"New Feature\") AND status in (Closed, Done, Resolved)")]
        public string Jql { get; set; }

        [Description("Url of Jira server")]
        public string JiraServer { get; set; }

        [Description("Url of YouTrack server")]
        public string YouTrackServer { get; set; }

        [Description("YouTrack filter for closed issues that you would like included if mentioned. Defaults to project:<YOURPROJECTID> State:Resolved State:-{{Won't fix}} State:-{{Can't Reproduce}} State:-Duplicate")]
        public string YouTrackFilter { get; set; }

        [Description("GitHub Repository name, in Organisation/Repository format")]
        public string Repo { get; set; }

        [Description("The release notes file")]
        public string OutputFile { get; set; }

        [Description("Allows additional labels to be treated as categories")]
        public string Categories { get; set; }

        [Description("Specifies the version to publish")]
        public string Version { get; set; }

        [Description("BitBuckets Consumer Key used for Oauth authentication")]
        public string ConsumerKey { get; set; }

        [Description("BitBuckets Consumer Secret Key used for Oauth authentication")]
        public string ConsumerSecretKey { get; set; }

        [Description("Specifies that all labels should be included in the release notes, if not specified then only the defaults (bug, enhancement, feature) are included.")]
        public bool AllLabels { get; set; }

    }
}