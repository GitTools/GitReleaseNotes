using System;
using GitReleaseNotes.IssueTrackers;

namespace GitReleaseNotes
{
    public static class GitReleaseNotesArgumentsExtensions
    {
        public static Context ToContext(this GitReleaseNotesArguments arguments)
        {
            IIssueTrackerContext issueTrackerContext;

            switch (arguments.IssueTracker)
            {
                case IssueTracker.BitBucket:
                    var bitBucketContext = new BitBucketContext
                    {
                    };

                    issueTrackerContext = bitBucketContext;
                    break;

                case IssueTracker.GitHub:
                    var gitHubContext = new GitHubContext
                    {
                    };

                    issueTrackerContext = gitHubContext;
                    break;

                case IssueTracker.Jira:
                    var jiraContext = new JiraContext
                    {
                        Jql = arguments.IssueTrackerFilter
                    };

                    issueTrackerContext = jiraContext;
                    break;

                case IssueTracker.YouTrack:
                    var youTrackContext = new YouTrackContext
                    {
                        Filter = arguments.IssueTrackerFilter
                    };

                    issueTrackerContext = youTrackContext;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            issueTrackerContext.Url = arguments.IssueTrackerUrl;
            issueTrackerContext.Username = arguments.IssueTrackerUsername;
            issueTrackerContext.Password = arguments.IssueTrackerPassword;
            issueTrackerContext.Token = arguments.IssueTrackerToken;
            issueTrackerContext.ProjectId = arguments.IssueTrackerProjectId;

            var context = new Context(issueTrackerContext);

            context.WorkingDirectory = arguments.WorkingDirectory;
            context.Verbose = arguments.Verbose;
            context.OutputFile = arguments.OutputFile;
            context.Categories = arguments.Categories;
            context.Version = arguments.Version;

            context.AllTags = arguments.AllTags;
            context.AllLabels = arguments.AllLabels;

            var repository = context.Repository;
            repository.Url = arguments.RepoUrl;
            repository.Branch = arguments.RepoBranch;
            repository.Username = arguments.RepoUsername;
            repository.Password = arguments.RepoPassword;
            repository.Token = arguments.RepoToken;

            return context;
        }
    }
}