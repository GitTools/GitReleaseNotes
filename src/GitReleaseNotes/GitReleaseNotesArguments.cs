using System.ComponentModel;
using GitTools.IssueTrackers;

namespace GitReleaseNotes
{
    public class GitReleaseNotesArguments
    {
        [Description("The directory of the Git repository to determine the version for; " +
                     "if unspecified it will search parent directories recursively until finding a Git repository.")]
        public string WorkingDirectory { get; set; }

        [Description("Enables verbose logging")]
        public bool Verbose { get; set; }

        [Description("Specifies the issue tracker used, possible Options: BitBucket, GitHub, Jira, YouTrack")]
        public IssueTracker? IssueTracker { get; set; }

        [Description("Specifies that all tags should be included in the release notes, if not specified then only the issues since the last tag are included.")]
        public bool AllTags { get; set; }

        [Description("Repository username")]
        public string RepoUsername { get; set; }

        [Description("Repository password")]
        public string RepoPassword { get; set; }

        [Description("Repository token (instead of username / password)")]
        public string RepoToken { get; set; }

        [Description("Url of repository")]
        public string RepoUrl { get; set; }

        [Description("The branch name to checkout any existing release notes file")]
        public string RepoBranch { get; set; }

        [Description("Issue tracker url")]
        public string IssueTrackerUrl{ get; set; }

        [Description("Issue tracker username")]
        public string IssueTrackerUsername { get; set; }

        [Description("Issue tracker password")]
        public string IssueTrackerPassword { get; set; }

        [Description("Issue tracker token (instead of username / password)")]
        public string IssueTrackerToken { get; set; }

        [Description("Issue tracker project ID")]
        public string IssueTrackerProjectId { get; set; }

        [Description("Jql query for closed issues you would like included if mentioned. Defaults to project = <YOURPROJECTID> AND (issuetype = Bug OR issuetype = Story OR issuetype = \"New Feature\") AND status in (Closed, Done, Resolved)")]
        public string IssueTrackerFilter { get; set; }

        [Description("The release notes file")]
        public string OutputFile { get; set; }

        [Description("Allows additional labels to be treated as categories")]
        public string Categories { get; set; }

        [Description("Specifies the version to publish")]
        public string Version { get; set; }

        [Description("Specifies that all labels should be included in the release notes, if not specified then only the defaults (bug, enhancement, feature) are included.")]
        public bool AllLabels { get; set; }

    }
}