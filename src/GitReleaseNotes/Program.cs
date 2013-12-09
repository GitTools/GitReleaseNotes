using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using Args.Help;
using Args.Help.Formatters;
using GitReleaseNotes.Git;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.IssueTrackers.GitHub;
using LibGit2Sharp;
using Octokit;
using Credentials = Octokit.Credentials;
using Repository = LibGit2Sharp.Repository;

namespace GitReleaseNotes
{
    public class Program
    {
        public static Dictionary<IssueTracker, IIssueTracker> IssueTrackers;

        static int Main(string[] args)
        {
            var modelBindingDefinition = Args.Configuration.Configure<GitReleaseNotesArguments>();

            if (args.Any(a => a == "/?" || a == "?" || a.Equals("/help", StringComparison.InvariantCultureIgnoreCase)))
            {
                var help = new HelpProvider().GenerateModelHelp(modelBindingDefinition);
                var f = new ConsoleHelpFormatter();
                f.WriteHelp(help, Console.Out);

                return 0;
            }

            var arguments = modelBindingDefinition.CreateAndBind(args);

            if (arguments.IssueTracker == null)
            {
                Console.WriteLine("The IssueTracker argument must be provided, see help (/?) for possible options");
                return 1;
            }
            if (string.IsNullOrEmpty(arguments.OutputFile) || !arguments.OutputFile.EndsWith(".md"))
            {
                Console.WriteLine("Specify an output file (*.md)");
                return 1;
            }

            CreateIssueTrackers(arguments);
            var issueTracker = IssueTrackers[arguments.IssueTracker.Value];
            if (!issueTracker.VerifyArgumentsAndWriteErrorsToConsole(arguments))
                return 1;

            var workingDirectory = arguments.WorkingDirectory ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var gitDirectory = GitDirFinder.TreeWalkForGitDir(workingDirectory);
            if (string.IsNullOrEmpty(gitDirectory))
            {
                throw new Exception("Failed to find .git directory.");
            }

            Console.WriteLine("Git directory found at {0}", gitDirectory);

            var repositoryRoot = Directory.GetParent(gitDirectory).FullName;

            var gitHelper = new GitHelper();
            var gitRepo = new Repository(gitDirectory);
            var taggedCommitFinder = new TaggedCommitFinder(gitRepo, gitHelper);

            var tagToStartFrom = string.IsNullOrEmpty(arguments.FromTag) ?
                taggedCommitFinder.GetLastTaggedCommit() :
                taggedCommitFinder.GetTag(arguments.FromTag);

            var releases = new CommitGrouper().GetCommitsByRelease(gitRepo, tagToStartFrom);

            Console.WriteLine("Scanning {0} commits over {1} releases for issue numbers", releases.Sum(r=>r.Value.Count), releases.Count);

            if (arguments.Verbose)
            {
                foreach (var release in releases)
                {
                    foreach (var commit in release.Value)
                    {
                        Console.WriteLine("[{0}] {1}", release.Key.Name, commit.Message);
                    }
                }
            }

            var releaseNotes = issueTracker.ScanCommitMessagesForReleaseNotes(arguments, releases);

            new ReleaseNotesWriter(new FileSystem(), repositoryRoot).WriteReleaseNotes(arguments, releaseNotes);
            return 0;
        }

        private static void CreateIssueTrackers(GitReleaseNotesArguments arguments)
        {
            var gitHubClient = new GitHubClient(new ProductHeaderValue("GitReleaseNotes"))
            {
                Credentials = new Credentials(arguments.Token)
            };
            IssueTrackers = new Dictionary<IssueTracker, IIssueTracker>
            {
                {IssueTracker.GitHub, new GitHubIssueTracker(new IssueNumberExtractor(), gitHubClient)}
            };
        }
    }
}
