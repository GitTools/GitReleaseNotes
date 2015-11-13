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

        public ReleaseNotesGenerator(ReleaseNotesGenerationParameters generationParameters)
        {
            _generationParameters = generationParameters;
        }

        public async Task<SemanticReleaseNotes> GenerateReleaseNotesAsync(SemanticReleaseNotes releaseNotesToUpdate)
        {
            var repositoryDirectory = _generationParameters.WorkingDirectory;
            if (!string.IsNullOrEmpty(repositoryDirectory))
            {
                Log.WriteLine("Looking for a git repository in '{0}'", repositoryDirectory);

                repositoryDirectory = Repository.Discover(repositoryDirectory);
            }

            if (string.IsNullOrEmpty(repositoryDirectory))
            {
                Log.WriteLine("No git repository found, trying to create a dynamic repository");

                var gitPreparer = new GitPreparer(_generationParameters.Repository.Url, null, _generationParameters.Repository.Authentication,
                    true, _generationParameters.WorkingDirectory);
                gitPreparer.Initialise(true, _generationParameters.Repository.Branch);

                repositoryDirectory = gitPreparer.GetDotGitDirectory();
            }

            var discoveredRepository = Repository.Discover(repositoryDirectory);
            if (discoveredRepository == null)
            {
                throw new Exception("Unable to find the .git directory (either on disk or dynamically)");
            }

            var gitRepository = new Repository(discoveredRepository);

            var issueTracker = CreateIssueTracker(gitRepository);
            if (issueTracker == null)
            {
                throw new Exception("Unable to determine the issue tracker, specify issue tracker type on the command line");
            }

            var categories = new Categories(_generationParameters.Categories, _generationParameters.AllLabels);
            var tagToStartFrom = _generationParameters.AllTags
                ? gitRepository.GetFirstCommit()
                : gitRepository.GetLastTaggedCommit() ?? gitRepository.GetFirstCommit();
            var currentReleaseInfo = gitRepository.GetCurrentReleaseInfo();
            if (!string.IsNullOrEmpty(_generationParameters.Version))
            {
                currentReleaseInfo.Name = _generationParameters.Version;
                currentReleaseInfo.When = DateTimeOffset.Now;
            }
            else
            {
                currentReleaseInfo.Name = "vNext";
            }

            var releaseNotes = await GenerateReleaseNotesAsync(
                _generationParameters, gitRepository, issueTracker,
                releaseNotesToUpdate, categories,
                tagToStartFrom, currentReleaseInfo);

            return releaseNotes;
        }

        private IIssueTracker CreateIssueTracker(IRepository gitRepository)
        {
            IIssueTracker issueTracker = null;

            // Step 1: try to use specified issue tracker
            if (_generationParameters.IssueTracker.Type.HasValue)
            {
                Log.WriteLine("Creating issue tracker based on specified issue tracker");

                issueTracker = IssueTrackerFactory.CreateIssueTracker(new IssueTrackerSettings(_generationParameters.IssueTracker.Server,
                        _generationParameters.IssueTracker.Type.Value)
                {
                    Project = _generationParameters.IssueTracker.ProjectId,
                    Authentication = _generationParameters.IssueTracker.Authentication.ToIssueTrackerSettings()
                });
            }

            // Step 2: try to determine from issue tracker url
            if (issueTracker == null)
            {
                if (!string.IsNullOrEmpty(_generationParameters.IssueTracker.Server))
                {
                    Log.WriteLine("Trying to determine issue tracker based on issue tracker url");

                    if (IssueTrackerFactory.TryCreateIssueTrackerFromUrl(
                        _generationParameters.IssueTracker.Server,
                        _generationParameters.IssueTracker.ProjectId,
                        _generationParameters.IssueTracker.Authentication.ToIssueTrackerSettings(),
                        out issueTracker))
                    {
                        Log.WriteLine("Determined issue tracker based on issue tracker url");
                    }
                    else
                    {
                        Log.WriteLine("Could not determine issue tracker based on issue tracker url");
                    }
                }
            }

            // Step 3: try to determine from repository url
            if (issueTracker == null)
            {
                Log.WriteLine("Trying to determine issue tracker based on repository url");

                if (!TryRemote(gitRepository, "upstream", _generationParameters, out issueTracker) &&
                    !TryRemote(gitRepository, "origin", _generationParameters, out issueTracker))
                {
                    Log.WriteLine("Determined issue tracker based on repository url");
                }
                else
                {
                    Log.WriteLine("Could not determine issue tracker based on repository url");
                }
            }

            if (issueTracker != null)
            {
                Log.WriteLine("Using issue tracker '{0}'", issueTracker.GetType().Name);
            }

            return issueTracker;
        }

        private static bool TryRemote(IRepository gitRepository, string name, ReleaseNotesGenerationParameters parameters,
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
                parameters.IssueTracker.ProjectId,
                parameters.IssueTracker.Authentication.ToIssueTrackerSettings(),
                out issueTracker);
        }

        // TODO: Why is generationParameters not used?
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