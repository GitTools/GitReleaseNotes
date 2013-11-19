using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Args.Help;
using Args.Help.Formatters;
using GitReleaseNotes.Git;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.IssueTrackers.GitHub;
using LibGit2Sharp;

namespace GitReleaseNotes
{
    public class Program
    {
        public static readonly Dictionary<IssueTracker, IIssueTracker> IssueTrackers = new Dictionary<IssueTracker, IIssueTracker>
        {
            {IssueTracker.GitHub, new GitHubIssueTracker(new IssueNumberExtractor())}
        };

        static int Main(string[] args)
        {
            var modelBindingDefinition = Args.Configuration.Configure<GitReleaseNotesArguments>();

            if (args.Any(a => a == "/?"))
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
            var commitsToScan = gitRepo.Commits.TakeWhile(c => c != tagToStartFrom.Commit).ToArray();

            if (arguments.Verbose)
            {
                Console.WriteLine("Scanning the following commits for issue numbers");
                foreach (var commit in commitsToScan)
                {
                    Console.WriteLine(commit.Message);
                }
            }

            var releaseNotes = issueTracker.ScanCommitMessagesForReleaseNotes(arguments, commitsToScan);

            new ReleaseNotesWriter(new FileSystem()).WriteReleaseNotes(arguments, releaseNotes);
            return 0;
        }
    }
}
