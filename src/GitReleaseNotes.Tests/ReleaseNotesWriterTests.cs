using System;
using System.IO;
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
        private readonly ReleaseNotesWriter _sut;
        private const string WorkingDir = "c:\\WorkingDir";

        public ReleaseNotesWriterTests()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _sut = new ReleaseNotesWriter(_fileSystem, WorkingDir);
        }

        [Fact]
        public void ApproveSimpleTests()
        {
            var arguments = new GitReleaseNotesArguments
            {
                OutputFile = "ReleaseFile.md"
            };
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new[]
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new string[0])
                })
            });

            _sut.WriteReleaseNotes(arguments, releaseNotes);

            Approvals.Verify(GetContent());
        }

        [Fact]
        public void ItemIsCategorised()
        {
            var arguments = new GitReleaseNotesArguments
            {
                OutputFile = "ReleaseFile.md"
            };
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new[]
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"feature"})
                })
            });

            _sut.WriteReleaseNotes(arguments, releaseNotes);

            Approvals.Verify(GetContent());
        }
        
        [Fact]
        public void MultipleReleases()
        {
            var arguments = new GitReleaseNotesArguments
            {
                OutputFile = "ReleaseFile.md"
            };
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new[]
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"feature"})
                }),
                new SemanticRelease("1.2.0", new DateTimeOffset(2013, 12, 06, 0,0,0, new TimeSpan()), new []
                {
                    new ReleaseNoteItem("Issue 2", "#2", new Uri("http://github.com/org/repo/issues/2"),
                        new[] {"feature"}),
                        new ReleaseNoteItem("Issue 3", "#3", new Uri("http://github.com/org/repo/issues/3"),
                        new[] {"bug"})
                })
            });

            _sut.WriteReleaseNotes(arguments, releaseNotes);

            Approvals.Verify(GetContent());
        }

        [Fact]
        public void LabelOfBugIsCategorisedAsFix()
        {
            var arguments = new GitReleaseNotesArguments
            {
                OutputFile = "ReleaseFile.md"
            };
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new[]
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new[] {"bug"})
                })
            });

            _sut.WriteReleaseNotes(arguments, releaseNotes);

            Approvals.Verify(GetContent());
        }

        [Fact]
        public void AdditionalCategoriesCanBeSpecifiedOnCommandLine()
        {
            var arguments = new GitReleaseNotesArguments
            {
                Categories = "internal refactoring",
                OutputFile = "ReleaseFile.md"
            };
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new[]
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"internal refactoring"})
                })
            });

            _sut.WriteReleaseNotes(arguments, releaseNotes);

            Approvals.Verify(GetContent());
        }

        [Fact]
        public void RelativePathIsWrittenToRepositoryRoot()
        {
            var arguments = new GitReleaseNotesArguments
            {
                Categories = "internal refactoring",
                OutputFile = "ReleaseFile.md"
            };
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new[]
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new string[0])
                })
            });

            _sut.WriteReleaseNotes(arguments, releaseNotes);

            var fileName = _fileSystem.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "WriteAllText").GetArguments()[0];
            Assert.Equal(Path.Combine(WorkingDir, "ReleaseFile.md"), fileName);
        }

        [Fact]
        public void AbsolutePathIsWrittenToRepositoryRoot()
        {
            var arguments = new GitReleaseNotesArguments
            {
                Categories = "internal refactoring",
                OutputFile = "c:\\AnotherDir\\ReleaseFile.md"
            };
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new[]
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new string[0])
                })
            });

            _sut.WriteReleaseNotes(arguments, releaseNotes);

            var fileName = _fileSystem.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "WriteAllText").GetArguments()[0];
            Assert.Equal("c:\\AnotherDir\\ReleaseFile.md", fileName);
        }

        private object GetContent()
        {
            return _fileSystem.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "WriteAllText").GetArguments()[1];
        }
    }
}