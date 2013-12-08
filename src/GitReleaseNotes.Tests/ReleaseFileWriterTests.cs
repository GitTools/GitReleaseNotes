using System.IO;
using System.Linq;
using NSubstitute;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class ReleaseFileWriterTests
    {
        private readonly IFileSystem _fileSystem;
        private const string RepositoryRoot = "c:\\Repo";
        private readonly ReleaseFileWriter _sut;

        public ReleaseFileWriterTests()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _sut = new ReleaseFileWriter(_fileSystem, RepositoryRoot);
        }

        [Fact]
        public void RelativePathIsWrittenToRepositoryRoot()
        {
            var arguments = new GitReleaseNotesArguments
            {
                Categories = "internal refactoring",
                OutputFile = "ReleaseFile.md"
            };

            _sut.OutputReleaseNotesFile(RepositoryRoot, arguments);

            var fileName = _fileSystem.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "WriteAllText").GetArguments()[0];
            Assert.Equal(Path.Combine(RepositoryRoot, "ReleaseFile.md"), fileName);
        }

        [Fact]
        public void AbsolutePathIsWrittenToRepositoryRoot()
        {
            var arguments = new GitReleaseNotesArguments
            {
                Categories = "internal refactoring",
                OutputFile = "c:\\AnotherDir\\ReleaseFile.md"
            };

            _sut.OutputReleaseNotesFile(RepositoryRoot, arguments);

            var fileName = _fileSystem.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "WriteAllText").GetArguments()[0];
            Assert.Equal("c:\\AnotherDir\\ReleaseFile.md", fileName);
        }
    }
}