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

        [Description("Specifies the issue tracker used, possible Options: GitHub")]
        public IssueTracker? IssueTracker { get; set; }

        [Description("Speficy the tag name to start from, default is the last tag on master")]
        public string FromTag { get; set; }

        [Description("GitHub access token")]
        public string Token { get; set; }

        [Description("GitHub Repository name, in Organisation/Repository format")]
        public string Repo { get; set; }

        [Description("The release notes file")]
        public string OutputFile { get; set; }

        [Description("Allows additional labels to be treated as categories")]
        public string Categories { get; set; }
    }
}