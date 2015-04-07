namespace GitReleaseNotes
{
    public static class GitReleaseNotesArgumentsExtensions
    {
        public static Context ToContext(this GitReleaseNotesArguments arguments)
        {
            var context = new Context();

            context.WorkingDirectory = arguments.WorkingDirectory;
            context.Verbose = arguments.Verbose;
            context.IssueTracker = arguments.IssueTracker;
            context.OutputFile = arguments.OutputFile;
            context.ProjectId = arguments.ProjectId;
            context.Categories = arguments.Categories;
            context.Version = arguments.Version;
            context.AllTags = arguments.AllTags;
            context.AllLabels = arguments.AllLabels;

            var authentication = context.Authentication;
            authentication.Username = arguments.Username;
            authentication.Password = arguments.Password;

            var repository = context.Repository;
            repository.Url = arguments.RepoUrl;
            repository.Branch = arguments.RepoBranch;
            repository.Username = arguments.RepoUsername;
            repository.Password = arguments.RepoPassword;

            var gitHub = context.GitHub;
            gitHub.Repo = arguments.Repo;
            gitHub.Token = arguments.Token;

            var jira = context.Jira;
            jira.JiraServer = arguments.JiraServer;
            jira.Jql = arguments.Jql;

            var youTrack = context.YouTrack;
            youTrack.YouTrackServer = arguments.YouTrackServer;
            youTrack.YouTrackFilter = arguments.YouTrackFilter;

            var bitBucket = context.BitBucket;
            bitBucket.Repo = arguments.Repo;
            bitBucket.ConsumerKey = arguments.ConsumerKey;
            bitBucket.ConsumerSecretKey = arguments.ConsumerSecretKey;

            return context;
        }
    }
}