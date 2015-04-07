using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitReleaseNotes.FileSystem;
using GitReleaseNotes.Git;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.IssueTrackers.BitBucket;
using GitReleaseNotes.IssueTrackers.GitHub;
using GitReleaseNotes.IssueTrackers.Jira;
using GitReleaseNotes.IssueTrackers.YouTrack;
using LibGit2Sharp;
using Octokit;
using Credentials = LibGit2Sharp.Credentials;

namespace GitReleaseNotes
{
    public static class ReleaseNotesGenerator
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private static Dictionary<IssueTracker, IIssueTracker> _issueTrackers;

        public static SemanticReleaseNotes GenerateReleaseNotes(Context context)
        {
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
                    {
                        issueTracker = firstOrDefault.Value;
                    }
                }

                if (issueTracker == null)
                {
                    if (!_issueTrackers.ContainsKey(context.IssueTracker.Value))
                    {
                        throw new GitReleaseNotesException("{0} is not a known issue tracker", context.IssueTracker.Value);
                    }

                    issueTracker = _issueTrackers[context.IssueTracker.Value];
                }

                if (!issueTracker.VerifyArgumentsAndWriteErrorsToLog())
                {
                    throw new GitReleaseNotesException("Argument verification failed");
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
                var tagToStartFrom = context.AllTags
                    ? GitRepositoryInfoFinder.GetFirstCommit(gitRepo)
                    : GitRepositoryInfoFinder.GetLastTaggedCommit(gitRepo) ?? GitRepositoryInfoFinder.GetFirstCommit(gitRepo);
                var currentReleaseInfo = GitRepositoryInfoFinder.GetCurrentReleaseInfo(gitRepo);
                if (!string.IsNullOrEmpty(context.Version))
                {
                    currentReleaseInfo.Name = context.Version;
                    currentReleaseInfo.When = DateTimeOffset.Now;
                }

                var releaseNotes = GenerateReleaseNotes(
                    gitRepo, issueTracker,
                    previousReleaseNotes, categories,
                    tagToStartFrom, currentReleaseInfo,
                    issueTracker.DiffUrlFormat);

                var releaseNotesOutput = releaseNotes.ToString();
                releaseFileWriter.OutputReleaseNotesFile(releaseNotesOutput, outputFile);

                return releaseNotes;
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
                        var gitHubClient = new GitHubClient(new ProductHeaderValue("GitReleaseNotes"));
                        if (context.GitHub.Token != null)
                        {
                            gitHubClient.Credentials = new Octokit.Credentials(context.GitHub.Token);
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

        public static SemanticReleaseNotes GenerateReleaseNotes(
            IRepository gitRepo, IIssueTracker issueTracker, SemanticReleaseNotes previousReleaseNotes,
            Categories categories, TaggedCommit tagToStartFrom, ReleaseInfo currentReleaseInfo,
            string diffUrlFormat)
        {
            var releases = ReleaseFinder.FindReleases(gitRepo, tagToStartFrom, currentReleaseInfo);
            var findIssuesSince =
                IssueStartDateBasedOnPreviousReleaseNotes(gitRepo, previousReleaseNotes)
                ??
                tagToStartFrom.Commit.Author.When;

            var closedIssues = issueTracker.GetClosedIssues(findIssuesSince).ToArray();

            var semanticReleases = (
                from release in releases
                let releaseNoteItems = closedIssues
                    .Where(i => (release.When == null || i.DateClosed < release.When) && (release.PreviousReleaseDate == null || i.DateClosed > release.PreviousReleaseDate))
                    .Select(i => new ReleaseNoteItem(i.Title, i.Id, i.HtmlUrl, i.Labels, i.DateClosed, i.Contributors))
                    .ToList<IReleaseNoteLine>()
                let beginningSha = release.FirstCommit == null ? null : release.FirstCommit.Substring(0, 10)
                let endSha = release.LastCommit == null ? null : release.LastCommit.Substring(0, 10)
                select new SemanticRelease(release.Name, release.When, releaseNoteItems, new ReleaseDiffInfo
                {
                    BeginningSha = beginningSha,
                    EndSha = endSha,
                    DiffUrlFormat = diffUrlFormat
                })).ToList();

            return new SemanticReleaseNotes(semanticReleases, categories).Merge(previousReleaseNotes);
        }

        private static DateTimeOffset? IssueStartDateBasedOnPreviousReleaseNotes(IRepository gitRepo,
            SemanticReleaseNotes previousReleaseNotes)
        {
            var lastGeneratedRelease = previousReleaseNotes.Releases.FirstOrDefault();
            if (lastGeneratedRelease == null) return null;
            var endSha = lastGeneratedRelease.DiffInfo.EndSha;
            if (string.IsNullOrEmpty(endSha))
            {
                lastGeneratedRelease = previousReleaseNotes.Releases.Skip(1).FirstOrDefault();
                if (lastGeneratedRelease != null)
                {
                    endSha = lastGeneratedRelease.DiffInfo.EndSha;
                }
            }

            if (string.IsNullOrEmpty(endSha)) return null;
            var commitToStartFrom = gitRepo.Commits.FirstOrDefault(c => c.Sha.StartsWith(endSha));
            if (commitToStartFrom != null)
            {
                return commitToStartFrom.Author.When;
            }
            return null;
        }
    }
}