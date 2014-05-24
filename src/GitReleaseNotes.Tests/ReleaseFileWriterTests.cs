using System.Linq;
using GitReleaseNotes.FileSystem;
using NSubstitute;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class ReleaseFileWriterTests
    {
        private readonly IFileSystem _fileSystem;
        private readonly ReleaseFileWriter _sut;

        public ReleaseFileWriterTests()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _sut = new ReleaseFileWriter(_fileSystem);
        }

        [Fact]
        public void AbsolutePathIsWrittenToRepositoryRoot()
        {
            _sut.OutputReleaseNotesFile("Contents", "c:\\AnotherDir\\ReleaseFile.md");

            var fileName = _fileSystem.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "WriteAllText").GetArguments()[0];
            Assert.Equal("c:\\AnotherDir\\ReleaseFile.md", fileName);
        }
    }
}