using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GitReleaseNotes
{
    public class ReleaseNotesWriter
    {
        private readonly IFileSystem _fileSystem;
        private readonly string[] _categories = { "bug", "enhancement", "feature" };
        private readonly string _workingDirectory;

        public ReleaseNotesWriter(IFileSystem fileSystem, string workingDirectory)
        {
            _fileSystem = fileSystem;
            _workingDirectory = workingDirectory;
        }

        public void WriteReleaseNotes(GitReleaseNotesArguments arguments, SemanticReleaseNotes releaseNotes)
        {
            var builder = new StringBuilder();
            var categories = arguments.Categories == null ? _categories : _categories.Concat(arguments.Categories.Split(',')).ToArray();
            foreach (var releaseNoteItem in releaseNotes.ReleaseNoteItems)
            {
                var taggedCategory = releaseNoteItem.Tags
                    .FirstOrDefault(t => categories.Any(c => c.Equals(t, StringComparison.InvariantCultureIgnoreCase)));
                var title = releaseNoteItem.Title;
                var issueNumber = releaseNoteItem.IssueNumber;
                var htmlUrl = releaseNoteItem.HtmlUrl;
                if ("bug".Equals(taggedCategory, StringComparison.InvariantCultureIgnoreCase))
                    taggedCategory = "fix";
                var category = taggedCategory == null ? null : string.Format(" +{0}", taggedCategory.Replace(" ", "-"));
                var item = string.Format(" - {0} [{1}]({2}){3}", title, issueNumber, htmlUrl, category);
                builder.AppendLine(item);
            }

            var outputFile = Path.IsPathRooted(arguments.OutputFile) ? arguments.OutputFile : Path.Combine(_workingDirectory, arguments.OutputFile);
            _fileSystem.WriteAllText(outputFile, builder.ToString());
        }
    }
}