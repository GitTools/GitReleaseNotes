using System.ComponentModel;

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
        public IssueTracker IssueTracker { get; set; }
    }
}