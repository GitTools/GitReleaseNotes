using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using Args.Help;
using Args.Help.Formatters;
using GitReleaseNotes.GenerationStrategy;
using GitReleaseNotes.Git;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.IssueTrackers.GitHub;
using GitReleaseNotes.IssueTrackers.Jira;
using LibGit2Sharp;
using Octokit;
using Credentials = Octokit.Credentials;
using Repository = LibGit2Sharp.Repository;

namespace GitReleaseNotes
{
    public static class Program
    {
        private static Dictionary<IssueTracker, IIssueTracker> _issueTrackers;

        static int Main(string[] args)
        {
            var main = GenerateReleaseNotes(args);
            Console.WriteLine("Done");
            if (Debugger.IsAttached)
                Console.ReadKey();
            return main;
        }

        private static int GenerateReleaseNotes(string[] args)
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

            if (!ArgumentVerifier.VerifyArguments(arguments))
            {
                return 1;
            }

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

            CreateIssueTrackers(gitRepo, arguments);

            IIssueTracker issueTracker = null;
            if (arguments.IssueTracker == null)
            {
                var firstOrDefault = _issueTrackers.FirstOrDefault(i => i.Value.RemotePresentWhichMatches);
                if (firstOrDefault.Value != null)
                    issueTracker = firstOrDefault.Value;
            }
            if (issueTracker == null)
            {
                if (!_issueTrackers.ContainsKey(arguments.IssueTracker.Value))
                    throw new Exception(string.Format("{0} is not a known issue tracker", arguments.IssueTracker.Value));

                issueTracker = _issueTrackers[arguments.IssueTracker.Value];
            }
            if (!issueTracker.VerifyArgumentsAndWriteErrorsToConsole())
            {
                return 1;
            }
            var taggedCommitFinder = new TaggedCommitFinder(gitRepo, gitHelper);

            TaggedCommit tagToStartFrom;
            if (string.IsNullOrEmpty(arguments.FromTag))
                tagToStartFrom = taggedCommitFinder.GetLastTaggedCommit();
            else if (arguments.FromTag == "all")
                tagToStartFrom = null;
            else
                tagToStartFrom = taggedCommitFinder.GetTag(arguments.FromTag);

            var releases = new CommitGrouper().GetCommitsByRelease(
                gitRepo, 
                tagToStartFrom,
                !string.IsNullOrEmpty(arguments.Version) ? new ReleaseInfo(arguments.Version, DateTimeOffset.Now, null, null) : null);

            IReleaseNotesStrategy generationStrategy;
            if (arguments.FromClosedIssues)
                generationStrategy = new ByClosedIssues();
            else if (arguments.FromMentionedIssues)
                generationStrategy = new ByMentionedCommits(new IssueNumberExtractor());
            else
                generationStrategy = new ByClosedIssues();

            var releaseNotes = generationStrategy.GetReleaseNotes(releases, arguments, issueTracker);

            var fileSystem = new FileSystem();
            var releaseNotesWriter = new ReleaseNotesGenerator();
            string outputFile = null;
            var previousReleaseNotes = new SemanticReleaseNotes();
            var releaseFileWriter = new ReleaseFileWriter(fileSystem);
            if (!string.IsNullOrEmpty(arguments.OutputFile))
            {
                outputFile = Path.IsPathRooted(arguments.OutputFile) ? arguments.OutputFile : Path.Combine(repositoryRoot, arguments.OutputFile);
                previousReleaseNotes = new ReleaseNotesFileReader(fileSystem, repositoryRoot).ReadPreviousReleaseNotes(outputFile);
            }

            var releaseNotesOutput = releaseNotesWriter.GenerateReleaseNotes(arguments, releaseNotes, previousReleaseNotes);
            releaseFileWriter.OutputReleaseNotesFile(releaseNotesOutput, outputFile);

            PublishReleaseIfNeeded(releaseNotesOutput, arguments, issueTracker);
            return 0;
        }

        private static void PublishReleaseIfNeeded(string releaseNotesOutput, GitReleaseNotesArguments arguments, IIssueTracker issueTracker)
        {
            if (!arguments.Publish)
                return;

            Console.WriteLine("Publishing release {0} to {1}", arguments.Version, arguments.IssueTracker);
            issueTracker.PublishRelease(releaseNotesOutput);
        }

        private static void CreateIssueTrackers(IRepository repository, GitReleaseNotesArguments arguments)
        {
            _issueTrackers = new Dictionary<IssueTracker, IIssueTracker>
            {
                {
                    IssueTracker.GitHub,
                    new GitHubIssueTracker(repository, () =>
                    {
                        var gitHubClient = new GitHubClient(new ProductHeaderValue("GitReleaseNotes"));
                        if (arguments.Token != null)
                        {
                            gitHubClient.Credentials = new Credentials(arguments.Token);
                        }

                        return gitHubClient;
                    }, new Log(), arguments)
                },
                {
                    IssueTracker.Jira, 
                    new JiraIssueTracker(new JiraApi(), arguments)
                }
            };
        }
    }
}
