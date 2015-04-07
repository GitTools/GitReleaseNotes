using Octokit;

namespace GitReleaseNotes.Tests.IssueTrackers.GitHub
{
    public class TestUser : User
    {
        public TestUser(string user, string name, string url)
        {
            Login = user;
            Name = name;
            HtmlUrl = url;
        }
    }
}