using System.Collections.Generic;
using System.Threading.Tasks;
using GitTools;
using GitTools.Git;
using GitTools.IssueTrackers;

namespace GitReleaseNotes
{
    using System;
    using System.IO;
    using System.Linq;
    using GitReleaseNotes.FileSystem;
    using GitReleaseNotes.Git;
    using LibGit2Sharp;

    public class ReleaseNotesGenerator
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly ReleaseNotesGenerationParameters _generationParameters;
        private readonly IFileSystem _fileSystem;

        public ReleaseNotesGenerator(ReleaseNotesGenerationParameters generationParameters, IFileSystem fileSystem)
        {
            _generationParameters = generationParameters;
            _fileSystem = fileSystem;
        }

        public async Task<SemanticReleaseNotes> GenerateReleaseNotesAsync()
        {
            var context = _generationParameters;

            using (var temporaryFilesContext = new TemporaryFilesContext())
            {
                //var gitPreparer = new GitPreparer();
                //var gitRepositoryDirectory = gitPreparer.Prepare(_generationParameters.RepositorySettings, temporaryFilesContext);
                var startingPath = context.WorkingDirectory ?? Directory.GetCurrentDirectory();
                var gitRepository = new Repository(Repository.Discover(startingPath));
                IIssueTracker issueTracker;
                if (context.IssueTracker.Type.HasValue)
                {
                    issueTracker = IssueTrackerFactory.CreateIssueTracker(new IssueTrackerSettings(context.IssueTracker.Server,
                            context.IssueTracker.Type.Value)
                        {
                            Project = context.IssueTracker.ProjectId,
                            Authentication = context.IssueTracker.Authentication
                        });
                }
                else
                {
                    if (!TryRemote(gitRepository, "upstream", context, out issueTracker) &&
                        !TryRemote(gitRepository, "origin", context, out issueTracker))
                    {
                        throw new Exception("Unable to guess issue tracker through remote, specify issue tracker type on the command line");
                    }
                }

                var releaseFileWriter = new ReleaseFileWriter(_fileSystem);
                string outputFile = null;
                var previousReleaseNotes = new SemanticReleaseNotes();

                var outputPath = startingPath;
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
                    ? gitRepository.GetFirstCommit()
                    : gitRepository.GetLastTaggedCommit() ?? gitRepository.GetFirstCommit();
                var currentReleaseInfo = gitRepository.GetCurrentReleaseInfo();
                if (!string.IsNullOrEmpty(context.Version))
                {
                    currentReleaseInfo.Name = context.Version;
                    currentReleaseInfo.When = DateTimeOffset.Now;
                }
                else
                {
                    currentReleaseInfo.Name = "vNext";
                }

                var releaseNotes = await GenerateReleaseNotesAsync(
                    context, gitRepository, issueTracker,
                    previousReleaseNotes, categories,
                    tagToStartFrom, currentReleaseInfo);

                var releaseNotesOutput = releaseNotes.ToString();
                releaseFileWriter.OutputReleaseNotesFile(releaseNotesOutput, outputFile);

                return releaseNotes;
            }
        }

        private static bool TryRemote(Repository gitRepository, string name, ReleaseNotesGenerationParameters context,
            out IIssueTracker issueTracker)
        {
            var upstream = gitRepository.Network.Remotes[name];
            if (upstream == null)
            {
                issueTracker = null;
                return false;
            }
            return IssueTrackerFactory.TryCreateIssueTrackerFromUrl(
                upstream.Url,
                context.IssueTracker.Authentication,
                out issueTracker);
        }

        public static async Task<SemanticReleaseNotes> GenerateReleaseNotesAsync(ReleaseNotesGenerationParameters generationParameters,
            IRepository gitRepo, IIssueTracker issueTracker, SemanticReleaseNotes previousReleaseNotes,
            Categories categories, TaggedCommit tagToStartFrom, ReleaseInfo currentReleaseInfo)
        {
            var releases = ReleaseFinder.FindReleases(gitRepo, tagToStartFrom, currentReleaseInfo);

            var findIssuesSince =
                IssueStartDateBasedOnPreviousReleaseNotes(gitRepo, previousReleaseNotes)
                ??
                tagToStartFrom.Commit.Author.When;

            var filter = new IssueTrackerFilter
            {
                Since = findIssuesSince,
                IncludeOpen = false
            };

            var closedIssues = (await issueTracker.GetIssuesAsync(filter)).ToArray();

            // As discussed here: https://github.com/GitTools/GitReleaseNotes/issues/85

            var semanticReleases = new Dictionary<string, SemanticRelease>();

            foreach (var issue in closedIssues)
            {
                // 1) Include all issues from the issue tracker that are assigned to this release
                foreach (var fixVersion in issue.FixVersions)
                {
                    if (!fixVersion.IsReleased)
                    {
                        continue;
                    }

                    if (!semanticReleases.ContainsKey(fixVersion.Name))
                    {
                        semanticReleases.Add(fixVersion.Name, new SemanticRelease(fixVersion.Name, fixVersion.ReleaseDate));
                    }

                    var semanticRelease = semanticReleases[fixVersion.Name];

                    var releaseNoteItem = new ReleaseNoteItem(issue.Title, issue.Id, issue.Url, issue.Labels,
                        issue.DateClosed, new Contributor[] { /*TODO: implement*/ });

                    semanticRelease.ReleaseNoteLines.Add(releaseNoteItem);
                }

                // 2) Get closed issues from the issue tracker that have no fixversion but are closed between the last release and this release
                if (issue.FixVersions.Count == 0)
                {
                    foreach (var release in releases)
                    {
                        if (issue.DateClosed.HasValue &&
                            issue.DateClosed.Value > release.PreviousReleaseDate &&
                            (release.When == null || issue.DateClosed <= release.When))
                        {
                            if (!semanticReleases.ContainsKey(release.Name))
                            {
                                var beginningSha = release.FirstCommit != null ? release.FirstCommit.Substring(0, 10) : null;
                                var endSha = release.LastCommit != null ? release.LastCommit.Substring(0, 10) : null;

                                semanticReleases.Add(release.Name, new SemanticRelease(release.Name, release.When, new ReleaseDiffInfo
                                {
                                    BeginningSha = beginningSha,
                                    EndSha = endSha,
                                    // TODO DiffUrlFormat = context.Repository.DiffUrlFormat
                                }));
                            }

                            var semanticRelease = semanticReleases[release.Name];

                            var releaseNoteItem = new ReleaseNoteItem(issue.Title, issue.Id, issue.Url, issue.Labels,
                                issue.DateClosed, issue.Contributors);

                            semanticRelease.ReleaseNoteLines.Add(releaseNoteItem);
                        }
                    }
                }
            }

            // 3) Remove any duplicates
            foreach (var semanticRelease in semanticReleases.Values)
            {
                var handledIssues = new HashSet<string>();

                for (var i = 0; i < semanticRelease.ReleaseNoteLines.Count; i++)
                {
                    var releaseNoteLine = semanticRelease.ReleaseNoteLines[i] as ReleaseNoteItem;
                    if (releaseNoteLine == null)
                    {
                        continue;
                    }

                    if (handledIssues.Contains(releaseNoteLine.IssueNumber))
                    {
                        semanticRelease.ReleaseNoteLines.RemoveAt(i--);
                        continue;
                    }

                    handledIssues.Add(releaseNoteLine.IssueNumber);
                }
            }

            var semanticReleaseNotes = new SemanticReleaseNotes(semanticReleases.Values, categories);
            var mergedReleaseNotes = semanticReleaseNotes.Merge(previousReleaseNotes);
            return mergedReleaseNotes;
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