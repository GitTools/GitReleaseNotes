using GitTools.Git;

namespace GitReleaseNotes.Git
{
    public class RepositoryInfo
    {
        public AuthenticationInfo Authentication { get; }

        public string Directory { get; set; }

        public string Branch { get; set; }

        public string Url { get; set; }
    }
}
