using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace GitReleaseNotes
{
    public static class Program
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private static Dictionary<IssueTracker, IIssueTracker> _issueTrackers;

        static int Main(string[] args)
        {
            GitReleaseNotesEnvironment.Log = new ConsoleLog();

            var main = GenerateReleaseNotes(args);

            Log.WriteLine("Done");

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }

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

            // TODO: Convert to context verification (we need the context to be valid, not the arguments)
            if (!ArgumentVerifier.VerifyArguments(arguments))
            {
                return 1;
            }

            var context = arguments.ToContext();

            using (var gitRepoContext = GetRepository(context))
            {
                // Remote repo's require some additional preparation before first use.
                if (gitRepoContext.IsRemote)
                {
                    gitRepoContext.PrepareRemoteRepoForUse(context.Repository.Branch);
                    if (!string.IsNullOrWhiteSpace(context.OutputFile))
                    {
                        gitRepoContext.CheckoutFilesIfExist(context.OutputFile);
                    }
                }

                var gitRepo = gitRepoContext.Repository;

                CreateIssueTrackers(gitRepo, context);

                IIssueTracker issueTracker = null;
                if (context.IssueTracker == null)
                {
                    var firstOrDefault = _issueTrackers.FirstOrDefault(i => i.Value.RemotePresentWhichMatches);
                    if (firstOrDefault.Value != null)
                        issueTracker = firstOrDefault.Value;
                }

                if (issueTracker == null)
                {
                    if (!_issueTrackers.ContainsKey(context.IssueTracker.Value))
                        throw new Exception(string.Format("{0} is not a known issue tracker", context.IssueTracker.Value));

                    issueTracker = _issueTrackers[context.IssueTracker.Value];
                }

                if (!issueTracker.VerifyArgumentsAndWriteErrorsToLog())
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

                if (!string.IsNullOrEmpty(context.OutputFile))
                {
                    outputFile = Path.IsPathRooted(context.OutputFile)
                        ? context.OutputFile
                        : Path.Combine(outputPath, context.OutputFile);
                    previousReleaseNotes = new ReleaseNotesFileReader(fileSystem, outputPath).ReadPreviousReleaseNotes(outputFile);
                }

                var categories = new Categories(context.Categories, context.AllLabels);
                TaggedCommit tagToStartFrom = context.AllTags
                    ? GitRepositoryInfoFinder.GetFirstCommit(gitRepo)
                    : GitRepositoryInfoFinder.GetLastTaggedCommit(gitRepo) ?? GitRepositoryInfoFinder.GetFirstCommit(gitRepo);
                var currentReleaseInfo = GitRepositoryInfoFinder.GetCurrentReleaseInfo(gitRepo);
                if (!string.IsNullOrEmpty(context.Version))
                {
                    currentReleaseInfo.Name = context.Version;
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

        private static void CreateIssueTrackers(IRepository repository, Context context)
        {
            _issueTrackers = new Dictionary<IssueTracker, IIssueTracker>
            {
                {
                    IssueTracker.GitHub,
                    new GitHubIssueTracker(repository, () =>
                    {
                        var gitHubClient = new GitHubClient(new Octokit.ProductHeaderValue("GitReleaseNotes"));
                        if (context.GitHub.Token != null)
                        {
                            gitHubClient.Credentials = new Credentials(context.GitHub.Token);
                        }

                        return gitHubClient;
                    }, context)
                },
                {
                    IssueTracker.Jira, 
                    new JiraIssueTracker(new JiraApi(), context)
                },
                {
                    IssueTracker.YouTrack,
                    new YouTrackIssueTracker(new YouTrackApi(), context)
                },
                {
                   IssueTracker.BitBucket,
                   new BitBucketIssueTracker(repository, new BitBucketApi(), context)
                }
            };
        }

        private static GitRepositoryContext GetRepository(Context context)
        {
            var workingDir = context.WorkingDirectory ?? Directory.GetCurrentDirectory();
            var isRemote = !string.IsNullOrWhiteSpace(context.Repository.Url);
            var repoFactory = GetRepositoryFactory(isRemote, workingDir, context);
            var repo = repoFactory.GetRepositoryContext();

            return repo;
        }

        private static IGitRepositoryContextFactory GetRepositoryFactory(bool isRemote, string workingDir, Context context)
        {
            IGitRepositoryContextFactory gitRepoFactory = null;
            if (isRemote)
            {
                // clone repo from the remote url
                var cloneRepoArgs = new GitRemoteRepositoryContextFactory.RemoteRepoArgs();
                cloneRepoArgs.Url = context.Repository.Url;
                var credentials = new UsernamePasswordCredentials();

                credentials.Username = context.Repository.Username;
                credentials.Password = context.Repository.Password;

                cloneRepoArgs.Credentials = credentials;
                cloneRepoArgs.DestinationPath = workingDir;

                Log.WriteLine("Cloning a git repo from {0}", cloneRepoArgs.Url);
                gitRepoFactory = new GitRemoteRepositoryContextFactory(cloneRepoArgs);
            }
            else
            {
                gitRepoFactory = new GitLocalRepositoryContextFactory(workingDir);
            }

            return gitRepoFactory;
        }
    }
}
