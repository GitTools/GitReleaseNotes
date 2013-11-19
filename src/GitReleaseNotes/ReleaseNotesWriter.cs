using System.Text;

namespace GitReleaseNotes
{
    public class ReleaseNotesWriter
    {
        private readonly IFileSystem _fileSystem;

        public ReleaseNotesWriter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void WriteReleaseNotes(GitReleaseNotesArguments arguments, SemanticReleaseNotes releaseNotes)
        {
            var builder = new StringBuilder();

            foreach (var releaseNoteItem in releaseNotes.ReleaseNoteItems)
            {
                var item = string.Format(" - {0} [{1}]({2})", releaseNoteItem.Title, releaseNoteItem.IssueNumber, releaseNoteItem.HtmlUrl);
                builder.AppendLine(item);
            }

            _fileSystem.WriteAllText(arguments.OutputFile, builder.ToString());
        }
    }
}