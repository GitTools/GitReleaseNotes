using System;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using GitReleaseNotes.IssueTrackers.GitHub;
using NSubstitute;
using Xunit;

namespace GitReleaseNotes.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class ReleaseNotesWriterTests
    {
        private readonly IFileSystem _fileSystem;

        public ReleaseNotesWriterTests()
        {
            _fileSystem = Substitute.For<IFileSystem>();
        }

        [Fact]
        public void ApproveSimpleTests()
        {
            var releaseNotesWriter = new ReleaseNotesWriter(_fileSystem);

            var arguments = new GitReleaseNotesArguments();
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new string[0]), 
            });

            releaseNotesWriter.WriteReleaseNotes(arguments, releaseNotes);

            var content = _fileSystem.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "WriteAllText").GetArguments()[1];
            Approvals.Verify(content);
        }
    }
}