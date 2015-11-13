using System;
using System.IO;
using GitTools;
using GitTools.Git;
using GitTools.IssueTrackers;

namespace GitReleaseNotes
{
    public static class GitReleaseNotesArgumentsExtensions
    {
        public static AuthSettings ToIssueTrackerSettings(this AuthenticationInfo authenticationInfo)
        {
            if (authenticationInfo.IsEmpty())
            {
                return new AuthSettings();
            }

            if (authenticationInfo.IsTokenAuthentication())
            {
                return new AuthSettings(authenticationInfo.Token);
            }

            if (authenticationInfo.IsUsernameAndPasswordAuthentication())
            {
                return new AuthSettings(authenticationInfo.Username, authenticationInfo.Password);
            }

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
                Repository =
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