using System.Linq;
using GitReleaseNotes.FileSystem;
using NSubstitute;
using Shouldly;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class ReleaseFileWriterTests
    {
        private readonly IFileSystem fileSystem;
        private readonly ReleaseFileWriter sut;

        public ReleaseFileWriterTests()
        {
            fileSystem = Substitute.For<IFileSystem>();
            sut = new ReleaseFileWriter(fileSystem);
        }

        [Fact]
        public void AbsolutePathIsWrittenToRepositoryRoot()
        {
            sut.OutputReleaseNotesFile("Contents", "c:\\AnotherDir\\ReleaseFile.md");

            var fileName = fileSystem.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "WriteAllText").GetArguments()[0];
            fileName.ShouldBe("c:\\AnotherDir\\ReleaseFile.md");
        }
    }
}