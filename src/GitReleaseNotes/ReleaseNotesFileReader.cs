using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitReleaseNotes
{
    public class ReleaseNotesFileReader
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _repositoryRoot;
        readonly Regex _issueRegex = new Regex(" - (?<Issue>.*?)(?<IssueLink> \\[(?<IssueId>.*?)\\]\\((?<IssueUrl>.*?)\\))*( *\\+(?<Tag>[^ \\+]*))*", RegexOptions.Compiled);
        readonly Regex _releaseRegex = new Regex("# (?<Title>.*?)( \\((?<Date>.*?)\\))?$", RegexOptions.Compiled);

        public ReleaseNotesFileReader(IFileSystem fileSystem, string repositoryRoot)
        {
            _fileSystem = fileSystem;
            _repositoryRoot = repositoryRoot;
        }

        public SemanticReleaseNotes ReadPreviousReleaseNotes(string releaseNotesFileName)
        {
            var path = Path.Combine(_repositoryRoot, releaseNotesFileName);
            if (!_fileSystem.FileExists(path))
                return new SemanticReleaseNotes();
            var contents = _fileSystem.ReadAllText(path).Replace("\r", string.Empty);
            var releases = new List<SemanticRelease>();
            var lines = contents.Split('\n');

            var currentRelease = new SemanticRelease();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.TrimStart().StartsWith("# "))
                {
                    var match = _releaseRegex.Match(line);
                    if (line != lines.First())
                        releases.Add(currentRelease);
                    currentRelease = new SemanticRelease
                    {
                        ReleaseName = match.Groups["Title"].Value
                    };

                    if (match.Groups["Date"].Success)
                        currentRelease.When = DateTime.ParseExact(match.Groups["Date"].Value, "dd MMMM yyyy", CultureInfo.CurrentCulture);
                }
                    //TODO Need to support multiple Url's in the release notes
                //else if (line.StartsWith(" - "))
                //{
                //    var match = _issueRegex.Match(line);
                //    var issue = match.Groups["Issue"].Value;
                //    var issueNumber = match.Groups["IssueId"].Value;
                //    var htmlUrl = match.Groups["IssueUrl"].Success ? new Uri(match.Groups["IssueUrl"].Value) : null;
                //    var tags = match.Groups["Tag"].Captures.OfType<Capture>().Select(c => c.Value).ToArray();
                //    var releaseNoteItem = new ReleaseNoteItem(issue, issueNumber, htmlUrl, tags, currentRelease.When);
                //    currentRelease.ReleaseNoteItems.Add(releaseNoteItem);
                //}
                else if (line.StartsWith("Commits: "))
                {
                    var commits = line.Replace("Commits: ", string.Empty).Split(new[] {"..."}, StringSplitOptions.None);
                    currentRelease.DiffInfo.BeginningSha = commits[0];
                    currentRelease.DiffInfo.EndSha = commits[1];
                }
                else
                {
                    // This picks up comments and such
                    var title = line.StartsWith(" - ") ? line.Substring(3) : line;
                    var releaseNoteItem = new ReleaseNoteItem(title, null, null, null, currentRelease.When, new Contributor[0]);
                    currentRelease.ReleaseNoteItems.Add(releaseNoteItem);
                }
            }

            releases.Add(currentRelease);

            return new SemanticReleaseNotes(releases);
        }
    }
}