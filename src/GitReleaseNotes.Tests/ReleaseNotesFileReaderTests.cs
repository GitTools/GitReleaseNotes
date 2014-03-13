using System;
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
            _fileSytem.FileExists(Arg.Any<string>()).Returns(true);

            _sut = new ReleaseNotesFileReader(_fileSytem, "c:\\RepoRoot");
        }

        [Fact]
        public void CanReadBasicReleaseNotes()
        {
            const string releaseNotes = @" - Issue 1 [#1](http://github.com/org/repo/issues/1)

Commits: 1234567...6789012
";
            _fileSytem.ReadAllText("c:\\RepoRoot\\ReleaseNotes.md").Returns(releaseNotes);

            var readReleaseNotes = _sut.ReadPreviousReleaseNotes("ReleaseNotes.md");

            readReleaseNotes.Releases.Length.ShouldBe(1);
            readReleaseNotes.Releases[0].DiffInfo.BeginningSha.ShouldBe("1234567");
            readReleaseNotes.Releases[0].DiffInfo.EndSha.ShouldBe("6789012");
            readReleaseNotes.Releases[0].ReleaseName.ShouldBe(null);
            readReleaseNotes.Releases[0].ReleaseNoteItems.Count.ShouldBe(1);
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].Title.ShouldBe("Issue 1 [#1](http://github.com/org/repo/issues/1)");
        }

        [Fact]
        public void CanReadReleaseNotesWithComments()
        {
            const string releaseNotes = @" - Issue 1 [#1](http://github.com/org/repo/issues/1)

Note: Some shiz..

Commits: 1234567...6789012
";
            _fileSytem.ReadAllText("c:\\RepoRoot\\ReleaseNotes.md").Returns(releaseNotes);

            var readReleaseNotes = _sut.ReadPreviousReleaseNotes("ReleaseNotes.md");

            readReleaseNotes.Releases.Length.ShouldBe(1);
            readReleaseNotes.Releases[0].DiffInfo.BeginningSha.ShouldBe("1234567");
            readReleaseNotes.Releases[0].DiffInfo.EndSha.ShouldBe("6789012");
            readReleaseNotes.Releases[0].ReleaseName.ShouldBe(null);
            readReleaseNotes.Releases[0].ReleaseNoteItems.Count.ShouldBe(2); 
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].Title.ShouldBe("Issue 1 [#1](http://github.com/org/repo/issues/1)");
            readReleaseNotes.Releases[0].ReleaseNoteItems[1].Title.ShouldBe("Note: Some shiz..");
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
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].Title.ShouldBe("Issue 1 [#1](http://github.com/org/repo/issues/1) +feature +new");
        }

        [Fact]
        public void CanReadReleaseNotesContainingMultipleReleases()
        {
            const string releaseNotes = @"# vNext

 - Issue 1 [#1](http://github.com/org/repo/issues/1) +feature +new

Commits: 12345678...67890123


# 1.2.0 (06 December 2013)

 - Issue 2 [#2](http://github.com/org/repo/issues/2) +feature
 - Issue 3 [#3](http://github.com/org/repo/issues/3) +fix

Commits: asdsadaf...bfdsadre
";
            _fileSytem.ReadAllText("c:\\RepoRoot\\ReleaseNotes.md").Returns(releaseNotes);

            var readReleaseNotes = _sut.ReadPreviousReleaseNotes("ReleaseNotes.md");

            readReleaseNotes.Releases.Length.ShouldBe(2);
            readReleaseNotes.Releases[0].DiffInfo.BeginningSha.ShouldBe("12345678");
            readReleaseNotes.Releases[0].DiffInfo.EndSha.ShouldBe("67890123");
            readReleaseNotes.Releases[0].ReleaseName.ShouldBe("vNext");
            readReleaseNotes.Releases[0].When.ShouldBe(null);
            readReleaseNotes.Releases[0].ReleaseNoteItems.Count.ShouldBe(1);
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].Title.ShouldBe("Issue 1 [#1](http://github.com/org/repo/issues/1) +feature +new");
            readReleaseNotes.Releases[1].DiffInfo.BeginningSha.ShouldBe("asdsadaf");
            readReleaseNotes.Releases[1].DiffInfo.EndSha.ShouldBe("bfdsadre");
            readReleaseNotes.Releases[1].ReleaseName.ShouldBe("1.2.0");
            readReleaseNotes.Releases[1].When.ShouldBe(new DateTimeOffset(new DateTime(2013, 12, 6)));
            readReleaseNotes.Releases[1].ReleaseNoteItems.Count.ShouldBe(2);
            readReleaseNotes.Releases[1].ReleaseNoteItems[0].Title.ShouldBe("Issue 2 [#2](http://github.com/org/repo/issues/2) +feature");
            readReleaseNotes.Releases[1].ReleaseNoteItems[1].Title.ShouldBe("Issue 3 [#3](http://github.com/org/repo/issues/3) +fix");
        }
    }
}