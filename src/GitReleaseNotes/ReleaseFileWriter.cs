using System.IO;

namespace GitReleaseNotes
{
    public class ReleaseFileWriter
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _repositoryRoot;

        public ReleaseFileWriter(IFileSystem fileSystem, string repositoryRoot)
        {
            _fileSystem = fileSystem;
            _repositoryRoot = repositoryRoot;
        }

        public void OutputReleaseNotesFile(string releaseNotesOutput, GitReleaseNotesArguments arguments)
        {
            if (string.IsNullOrEmpty(arguments.OutputFile))
                return;
            var outputFile = Path.IsPathRooted(arguments.OutputFile) ? arguments.OutputFile : Path.Combine(_repositoryRoot, arguments.OutputFile);
            _fileSystem.WriteAllText(outputFile, releaseNotesOutput);
        }
    }
}