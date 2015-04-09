namespace GitReleaseNotes
{
    using System;
    using System.IO;
    using System.Linq;
    using GitReleaseNotes.FileSystem;
    using GitReleaseNotes.Git;
    using GitReleaseNotes.IssueTrackers;
    using LibGit2Sharp;

    public class ReleaseNotesGenerator
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly Context _context;
        private readonly IFileSystem _fileSystem;
        private readonly IIssueTrackerFactory _issueTrackerFactory;

        public ReleaseNotesGenerator(Context context, IFileSystem fileSystem, IIssueTrackerFactory issueTrackerFactory)
        {
            _context = context;
            _fileSystem = fileSystem;
            _issueTrackerFactory = issueTrackerFactory;
        }

        public SemanticReleaseNotes GenerateReleaseNotes()
        {
            var context = _context;

            using (var gitRepoContext = GetRepository(context, _fileSystem))
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

                if (context.IssueTracker == null)
                {
                    // TODO: Write auto detection mechanism which is better than this
                    throw new GitReleaseNotesException("Feature to automatically detect issue tracker must be written");
                    //var firstOrDefault = _issueTrackers.FirstOrDefault(i => i.Value.RemotePresentWhichMatches);
                    //if (firstOrDefault.Value != null)
                    //{
                    //    issueTracker = firstOrDefault.Value;
                    //}
                }

                var issueTracker = _issueTrackerFactory.CreateIssueTracker(context, gitRepo);
                if (issueTracker == null)
                {
                    throw new GitReleaseNotesException("Failed to create issue tracker from context, cannot continue");
                }

                var releaseFileWriter = new ReleaseFileWriter(_fileSystem);
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
                    previousReleaseNotes = new ReleaseNotesFileReader(_fileSystem, outputPath).ReadPreviousReleaseNotes(outputFile);
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
                    context, gitRepo, issueTracker,
                    previousReleaseNotes, categories,
                    tagToStartFrom, currentReleaseInfo);

                var releaseNotesOutput = releaseNotes.ToString();
                releaseFileWriter.OutputReleaseNotesFile(releaseNotesOutput, outputFile);

                return releaseNotes;
            }
        }

        private GitRepositoryContext GetRepository(Context context, IFileSystem fileSystem)
        {
            var workingDir = _fileSystem.GetRepositoryWorkingDirectory(context);
            var isRemote = !string.IsNullOrWhiteSpace(context.Repository.Url);
            var repoFactory = GetRepositoryFactory(isRemote, workingDir, context, fileSystem);
            var repo = repoFactory.GetRepositoryContext();

            return repo;
        }

        private static IGitRepositoryContextFactory GetRepositoryFactory(bool isRemote, string workingDir, Context context, IFileSystem fileSystem)
        {
            IGitRepositoryContextFactory gitRepoFactory = null;
            if (isRemote)
            {
                // clone repo from the remote url
                var cloneRepoArgs = new GitRemoteRepositoryContextFactory.RemoteRepoArgs();
                cloneRepoArgs.Url = context.Repository.Url;
                cloneRepoArgs.Branch = context.Repository.Branch;

                var credentials = new UsernamePasswordCredentials();
                credentials.Username = context.Repository.Username;
                credentials.Password = context.Repository.Password;

                cloneRepoArgs.Credentials = credentials;
                cloneRepoArgs.DestinationPath = workingDir;

                Log.WriteLine("Cloning a git repo from {0}", cloneRepoArgs.Url);
                gitRepoFactory = new GitRemoteRepositoryContextFactory(cloneRepoArgs, fileSystem);
            }
            else
            {
                gitRepoFactory = new GitLocalRepositoryContextFactory(workingDir, fileSystem);
            }

            return gitRepoFactory;
        }

        public static SemanticReleaseNotes GenerateReleaseNotes(Context context,
            IRepository gitRepo, IIssueTracker issueTracker, SemanticReleaseNotes previousReleaseNotes,
            Categories categories, TaggedCommit tagToStartFrom, ReleaseInfo currentReleaseInfo)
        {
            var releases = ReleaseFinder.FindReleases(gitRepo, tagToStartFrom, currentReleaseInfo);
            var findIssuesSince =
                IssueStartDateBasedOnPreviousReleaseNotes(gitRepo, previousReleaseNotes)
                ??
                tagToStartFrom.Commit.Author.When;

            var closedIssues = issueTracker.GetClosedIssues(context.IssueTracker, findIssuesSince).ToArray();

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
                    //DiffUrlFormat = issueTracker.DiffUrlFormat
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