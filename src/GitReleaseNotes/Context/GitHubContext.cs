namespace GitReleaseNotes
{
    // TODO: Move some parts to authentication context
    public class GitHubContext
    {
        public string Token { get; set; }

        public string Repo { get; set; }

        public string RepoUsername { get; set; }

        public string RepoPassword { get; set; }
    }
}