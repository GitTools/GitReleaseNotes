using System;
using System.IO;
using GitTools;
using GitTools.IssueTrackers;

namespace GitReleaseNotes
{
    public static class GitReleaseNotesArgumentsExtensions
    {
        public static AuthSettings ToIssueTrackerSettings(this AuthenticationContext authenticationContext)
        {
            if (authenticationContext.IsEmpty())
                return new AuthSettings();
            if (authenticationContext.IsTokenAuthentication())
                return new AuthSettings(authenticationContext.Token);
            if (authenticationContext.IsUsernameAndPasswordAuthentication())
                return new AuthSettings(authenticationContext.Username, authenticationContext.Password);

            throw new ArgumentException("Authentication context has an unsupported configuration");
        }

        public static ReleaseNotesGenerationParameters ToParameters(this GitReleaseNotesArguments arguments)
        {
            return new ReleaseNotesGenerationParameters
            {
                WorkingDirectory = arguments.WorkingDirectory ?? Directory.GetCurrentDirectory(),
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