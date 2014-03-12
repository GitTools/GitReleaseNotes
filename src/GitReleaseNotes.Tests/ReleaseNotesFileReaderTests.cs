using System;
using System.Linq;
using NSubstitute;
using Shouldly;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class ReleaseNotesFileReaderTests
    {
        private readonly IFileSystem _fileSytem;
        private readonly ReleaseNotesFileReader _sut;

        public ReleaseNotesFileReaderTests()
        {
            _fileSytem = Substitute.For<IFileSystem>();

            _sut = new ReleaseNotesFileReader(_fileSytem, "c:\\RepoRoot");
        }

        [Fact]
        public void CanReadBasicReleaseNotes()
        {
            const string releaseNotes = @" - Issue 1 [#1](http://github.com/org/repo/issues/1)

Commits: 12345678...67890123
";
            _fileSytem.ReadAllText("c:\\RepoRoot\\ReleaseNotes.md").Returns(releaseNotes);

            var readReleaseNotes = _sut.ReadPreviousReleaseNotes("ReleaseNotes.md");

            readReleaseNotes.Releases.Length.ShouldBe(1);
            readReleaseNotes.Releases[0].DiffInfo.BeginningSha.ShouldBe("12345678");
            readReleaseNotes.Releases[0].DiffInfo.EndSha.ShouldBe("67890123");
            readReleaseNotes.Releases[0].ReleaseName.ShouldBe(null);
            readReleaseNotes.Releases[0].ReleaseNoteItems.Count.ShouldBe(1);
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].Title.ShouldBe("Issue 1");
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].IssueNumber.ShouldBe("#1");
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].HtmlUrl.ShouldBe(new Uri("http://github.com/org/repo/issues/1"));
        }

        [Fact]
        public void CanReadCategorisedIssuesReleaseNotes()
        {
            const string releaseNotes = @" - Issue 1 [#1](http://github.com/org/repo/issues/1) +feature +new

Commits: 12345678...67890123
";
            _fileSytem.ReadAllText("c:\\RepoRoot\\ReleaseNotes.md").Returns(releaseNotes);

            var readReleaseNotes = _sut.ReadPreviousReleaseNotes("ReleaseNotes.md");

            readReleaseNotes.Releases.Length.ShouldBe(1);
            readReleaseNotes.Releases[0].DiffInfo.BeginningSha.ShouldBe("12345678");
            readReleaseNotes.Releases[0].DiffInfo.EndSha.ShouldBe("67890123");
            readReleaseNotes.Releases[0].ReleaseName.ShouldBe(null);
            readReleaseNotes.Releases[0].ReleaseNoteItems.Count.ShouldBe(1);
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].Title.ShouldBe("Issue 1");
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].IssueNumber.ShouldBe("#1");
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].HtmlUrl.ShouldBe(new Uri("http://github.com/org/repo/issues/1"));
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].Tags.ShouldBe(new[]{"feature", "new"});
        }
    }
}