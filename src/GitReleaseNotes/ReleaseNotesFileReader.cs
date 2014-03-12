using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitReleaseNotes
{
    public class ReleaseNotesFileReader
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _repositoryRoot;
        readonly Regex _issueRegex = new Regex(" - (?<Issue>.*?) \\[(?<IssueId>.*?)\\]\\((?<IssueUrl>.*?)\\)( *\\+(?<Tag>[^ \\+]*))*", RegexOptions.Compiled);

        public ReleaseNotesFileReader(IFileSystem fileSystem, string repositoryRoot)
        {
            _fileSystem = fileSystem;
            _repositoryRoot = repositoryRoot;
        }

        public SemanticReleaseNotes ReadPreviousReleaseNotes(string releaseNotesFileName)
        {
            var path = Path.Combine(_repositoryRoot, releaseNotesFileName);
            var contents = _fileSystem.ReadAllText(path).Replace("\r", string.Empty);
            var releases = new List<SemanticRelease>();
            var lines = contents.Split('\n');

            var currentRelease = new SemanticRelease();
            foreach (var line in lines)
            {
                if (line.StartsWith(" - "))
                {
                    var match = _issueRegex.Match(line);
                    var issue = match.Groups["Issue"].Value;
                    var issueNumber = match.Groups["IssueId"].Value;
                    var htmlUrl = new Uri(match.Groups["IssueUrl"].Value);
                    var tags = match.Groups["Tag"].Captures.OfType<Capture>().Select(c => c.Value).ToArray();
                    var releaseNoteItem = new ReleaseNoteItem(issue, issueNumber, htmlUrl, tags);
                    currentRelease.ReleaseNoteItems.Add(releaseNoteItem);
                }

                if (line.StartsWith("Commits: "))
                {
                    var commits = line.Replace("Commits: ", string.Empty).Split(new []{"..."}, StringSplitOptions.None);
                    currentRelease.DiffInfo.BeginningSha = commits[0];
                    currentRelease.DiffInfo.EndSha = commits[1];
                }
            }

            releases.Add(currentRelease);

            return new SemanticReleaseNotes(releases);
        }
    }
}