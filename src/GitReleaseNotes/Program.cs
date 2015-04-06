using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Args.Help;
using Args.Help.Formatters;
using GitReleaseNotes.FileSystem;
using GitReleaseNotes.Git;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.IssueTrackers.BitBucket;
using GitReleaseNotes.IssueTrackers.GitHub;
using GitReleaseNotes.IssueTrackers.Jira;
using GitReleaseNotes.IssueTrackers.YouTrack;
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

            using (var gitRepoContext = GetRepository(arguments))
            {
                // Remote repo's require some additional preparation before first use.
                if (gitRepoContext.IsRemote)
                {
                    gitRepoContext.PrepareRemoteRepoForUse(arguments.RepoBranch);
                    if (!string.IsNullOrWhiteSpace(arguments.OutputFile))
                    {
                        gitRepoContext.CheckoutFilesIfExist(arguments.OutputFile);
                    }                   
                }
                var gitRepo = gitRepoContext.Repository;

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

                var fileSystem = new FileSystem.FileSystem();
                var releaseFileWriter = new ReleaseFileWriter(fileSystem);
                string outputFile = null;
                var previousReleaseNotes = new SemanticReleaseNotes();

                var outputPath = gitRepo.Info.Path;
                var outputDirectory = new DirectoryInfo(outputPath);
                if (outputDirectory.Name == ".git")
                {
                    outputPath = outputDirectory.Parent.FullName;
                }

                if (!string.IsNullOrEmpty(arguments.OutputFile))
                {
                    outputFile = Path.IsPathRooted(arguments.OutputFile)
                        ? arguments.OutputFile
                        : Path.Combine(outputPath, arguments.OutputFile);
                    previousReleaseNotes = new ReleaseNotesFileReader(fileSystem, outputPath).ReadPreviousReleaseNotes(outputFile);
                }

                var categories = new Categories(arguments.Categories, arguments.AllLabels);
                TaggedCommit tagToStartFrom = arguments.AllTags
                    ? GitRepositoryInfoFinder.GetFirstCommit(gitRepo)
                    : GitRepositoryInfoFinder.GetLastTaggedCommit(gitRepo) ?? GitRepositoryInfoFinder.GetFirstCommit(gitRepo);
                var currentReleaseInfo = GitRepositoryInfoFinder.GetCurrentReleaseInfo(gitRepo);
                if (!string.IsNullOrEmpty(arguments.Version))
                {
                    currentReleaseInfo.Name = arguments.Version;
                    currentReleaseInfo.When = DateTimeOffset.Now;
                }
                var releaseNotes = ReleaseNotesGenerator.GenerateReleaseNotes(
                    gitRepo, issueTracker,
                    previousReleaseNotes, categories,
                    tagToStartFrom, currentReleaseInfo,
                    issueTracker.DiffUrlFormat);

                var releaseNotesOutput = releaseNotes.ToString();
                releaseFileWriter.OutputReleaseNotesFile(releaseNotesOutput, outputFile);

                return 0;

            }
        }

        private static void CreateIssueTrackers(IRepository repository, GitReleaseNotesArguments arguments)
        {
            var log = new Log();
            _issueTrackers = new Dictionary<IssueTracker, IIssueTracker>
            {
                {
                    IssueTracker.GitHub,
                    new GitHubIssueTracker(repository, () =>
                    {
                        var gitHubClient = new GitHubClient(new Octokit.ProductHeaderValue("GitReleaseNotes"));
                        if (arguments.Token != null)
                        {
                            gitHubClient.Credentials = new Credentials(arguments.Token);
                        }

                        return gitHubClient;
                    }, log, arguments)
                },
                {
                    IssueTracker.Jira, 
                    new JiraIssueTracker(new JiraApi(), log, arguments)
                },
                {
                    IssueTracker.YouTrack,
                    new YouTrackIssueTracker(new YouTrackApi(), log, arguments)
                },
                {
                   IssueTracker.BitBucket,
                   new BitBucketIssueTracker(repository, new BitBucketApi(), log, arguments)
                }
            };
        }

        private static GitRepositoryContext GetRepository(GitReleaseNotesArguments args)
        {
            var workingDir = args.WorkingDirectory ?? Directory.GetCurrentDirectory();
            bool isRemote = !string.IsNullOrWhiteSpace(args.RepoUrl);
            ILog log = new Log();
            IGitRepositoryContextFactory repoFactory = GetRepositoryFactory(log, isRemote, workingDir, args);
            var repo = repoFactory.GetRepositoryContext();
            return repo;
        }

        private static IGitRepositoryContextFactory GetRepositoryFactory(ILog log, bool isRemote, string workingDir, GitReleaseNotesArguments args)
        {
            IGitRepositoryContextFactory gitRepoFactory = null;
            if (isRemote)
            {

                // clone repo from the remote url
                var cloneRepoArgs = new GitReleaseNotes.Git.GitRemoteRepositoryContextFactory.RemoteRepoArgs();
                cloneRepoArgs.Url = args.RepoUrl;
                var credentials = new UsernamePasswordCredentials();
                credentials.Username = args.RepoUsername;
                credentials.Password = args.RepoPassword;
                cloneRepoArgs.Credentials = credentials;
                cloneRepoArgs.DestinationPath = workingDir;
               
                Console.WriteLine("Cloning a git repo from {0}", cloneRepoArgs.Url);
                gitRepoFactory = new GitRemoteRepositoryContextFactory(log, cloneRepoArgs);
            }
            else
            {
                gitRepoFactory = new GitLocalRepositoryContextFactory(log, workingDir);
            }
            return gitRepoFactory;
        }
    }
}
