namespace GitReleaseNotes
{
    public class RepositoryContext : AuthenticationContext
    {
        public string Url { get; set; }

        public string Branch { get; set; }
    }
}