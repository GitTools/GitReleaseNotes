using GitTools;

namespace GitReleaseNotes
{
    public static class GitReleaseNotesArgumentsExtensions
    {
        public static ReleaseNotesGenerationParameters ToContext(this GitReleaseNotesArguments arguments)
        {
            return new ReleaseNotesGenerationParameters
            {
                WorkingDirectory = arguments.WorkingDirectory,
                OutputFile = arguments.OutputFile,
                Categories = arguments.Categories,
                Version = arguments.Version,
                AllTags = arguments.AllTags,
                AllLabels = arguments.AllLabels,
                RepositorySettings =
                {
                    Url = arguments.RepoUrl,
                    Branch = arguments.RepoBranch,
                    Authentication =
                    {
                        Username = arguments.RepoUsername,
                        Password = arguments.RepoPassword,
                        Token = arguments.RepoToken
                    }
                },
                IssueTracker =
                {
                    Server = arguments.IssueTrackerUrl,
                    ProjectId = arguments.IssueTrackerProjectId,
                    Type = arguments.IssueTracker,
                    Authentication =
                    {
                        Username = arguments.IssueTrackerUsername,
                        Password = arguments.IssueTrackerPassword,
                        Token = arguments.IssueTrackerToken
                    }
                }
            };
        }
    }
}