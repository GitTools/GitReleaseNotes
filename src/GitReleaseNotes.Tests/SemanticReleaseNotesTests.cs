using System;
using System.Collections.Generic;
using ApprovalTests;
using Shouldly;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class SemanticReleaseNotesTests
    {
        [Fact]
        public void ApproveSimpleTests()
        {
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new List<IReleaseNoteLine>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new string[0], DateTime.Now, new[]{ new Contributor("Foo Bar", "@foo", "http://url.com/foo") }),
                    new ReleaseNoteItem("Issue 1", null, null, new string[0], DateTime.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            }, new Categories());

            var result = releaseNotes.ToString();

            Approvals.Verify(result);
        }

        [Fact]
        public void ItemIsCategorised()
        {
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new List<IReleaseNoteLine>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"feature"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            }, new Categories(new[] { "feature" }, true));

            var result = releaseNotes.ToString();

            Approvals.Verify(result);
        }

        [Fact]
        public void ItemIsCategorisedWithMultipleCategoriesIfAllLabelsIsTrue()
        {
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new List<IReleaseNoteLine>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"feature", "enhancement", "breaking-change"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            }, new Categories("feature", true));

            var result = releaseNotes.ToString();

            Approvals.Verify(result);
        }

        [Fact]
        public void ItemIsNotCategorisedWithMultipleCategoriesIfAllLabelsIsFalse()
        {
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new List<IReleaseNoteLine>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"feature", "enhancement", "breaking-change"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            }, new Categories("feature", false));

            var result = releaseNotes.ToString();

            Approvals.Verify(result);
        }

        [Fact]
        public void MultipleReleases()
        {
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new List<IReleaseNoteLine>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"feature"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                }),
                new SemanticRelease("1.2.0", new DateTimeOffset(2013, 12, 06, 0,0,0, new TimeSpan()), new List<IReleaseNoteLine>
                {
                    new ReleaseNoteItem("Issue 2", "#2", new Uri("http://github.com/org/repo/issues/2"),
                        new[] {"feature"}, DateTimeOffset.Now, new Contributor[0]),
                        new ReleaseNoteItem("Issue 3", "#3", new Uri("http://github.com/org/repo/issues/3"),
                        new[] {"bug"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "asdsadaf",
                    EndSha = "bfdsadre"
                })
            }, new Categories(new[] { "bug", "enhancement", "feature" }, true));

            var result = releaseNotes.ToString();

            Approvals.Verify(result);
        }

        [Fact]
        public void LabelOfBugIsCategorisedAsFix()
        {
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new List<IReleaseNoteLine>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new[] {"bug"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            }, new Categories(new[] { "bug", "enhancement", "feature" }, true));

            var result = releaseNotes.ToString();

            Approvals.Verify(result);
        }

        [Fact]
        public void AdditionalCategoriesCanBeSpecifiedOnCommandLine()
        {
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new List<IReleaseNoteLine>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"internal refactoring"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            }, new Categories(new[] { "internal refactoring" }, true));

            var result = releaseNotes.ToString();

            Approvals.Verify(result);
        }

        [Fact]
        public void CanReadBasicReleaseNotes()
        {
            const string releaseNotes = @" - Issue 1 [#1](http://github.com/org/repo/issues/1)

Commits: 1234567...6789012
";

            var readReleaseNotes = SemanticReleaseNotes.Parse(releaseNotes);

            readReleaseNotes.Releases.Length.ShouldBe(1);
            readReleaseNotes.Releases[0].DiffInfo.BeginningSha.ShouldBe("1234567");
            readReleaseNotes.Releases[0].DiffInfo.EndSha.ShouldBe("6789012");
            readReleaseNotes.Releases[0].ReleaseName.ShouldBe(null);
            readReleaseNotes.Releases[0].ReleaseNoteLines.Count.ShouldBe(1);
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].Title.ShouldBe("Issue 1 [#1](http://github.com/org/repo/issues/1)");
        }

        [Fact]
        public void CanReadReleaseNotesWithComments()
        {
            const string releaseNotes = @" - Issue 1 [#1](http://github.com/org/repo/issues/1)

Note: Some shiz..

Commits: 1234567...6789012
";

            var readReleaseNotes = SemanticReleaseNotes.Parse(releaseNotes);

            readReleaseNotes.Releases.Length.ShouldBe(1);
            readReleaseNotes.Releases[0].DiffInfo.BeginningSha.ShouldBe("1234567");
            readReleaseNotes.Releases[0].DiffInfo.EndSha.ShouldBe("6789012");
            readReleaseNotes.Releases[0].ReleaseName.ShouldBe(null);
            readReleaseNotes.Releases[0].ReleaseNoteLines.Count.ShouldBe(3);
            readReleaseNotes.Releases[0].ReleaseNoteLines[0].ToString(new Categories()).ShouldBe(" - Issue 1 [#1](http://github.com/org/repo/issues/1)");
            readReleaseNotes.Releases[0].ReleaseNoteLines[1].ToString(new Categories()).ShouldBe(string.Empty);
            readReleaseNotes.Releases[0].ReleaseNoteLines[2].ToString(new Categories()).ShouldBe("Note: Some shiz..");
        }

        [Fact]
        public void CanReadCategorisedIssuesReleaseNotes()
        {
            const string releaseNotes = @" - Issue 1 [#1](http://github.com/org/repo/issues/1) +feature +new

Commits: 12345678...67890123
";

            var readReleaseNotes = SemanticReleaseNotes.Parse(releaseNotes);

            readReleaseNotes.Releases.Length.ShouldBe(1);
            readReleaseNotes.Releases[0].DiffInfo.BeginningSha.ShouldBe("12345678");
            readReleaseNotes.Releases[0].DiffInfo.EndSha.ShouldBe("67890123");
            readReleaseNotes.Releases[0].ReleaseName.ShouldBe(null);
            readReleaseNotes.Releases[0].ReleaseNoteItems.Length.ShouldBe(1);
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
            var readReleaseNotes = SemanticReleaseNotes.Parse(releaseNotes);

            readReleaseNotes.Releases.Length.ShouldBe(2);
            readReleaseNotes.Releases[0].DiffInfo.BeginningSha.ShouldBe("12345678");
            readReleaseNotes.Releases[0].DiffInfo.EndSha.ShouldBe("67890123");
            readReleaseNotes.Releases[0].ReleaseName.ShouldBe(null);
            readReleaseNotes.Releases[0].When.ShouldBe(null);
            readReleaseNotes.Releases[0].ReleaseNoteLines.Count.ShouldBe(1);
            readReleaseNotes.Releases[0].ReleaseNoteItems[0].Title.ShouldBe("Issue 1 [#1](http://github.com/org/repo/issues/1) +feature +new");
            readReleaseNotes.Releases[1].DiffInfo.BeginningSha.ShouldBe("asdsadaf");
            readReleaseNotes.Releases[1].DiffInfo.EndSha.ShouldBe("bfdsadre");
            readReleaseNotes.Releases[1].ReleaseName.ShouldBe("1.2.0");
            readReleaseNotes.Releases[1].When.ShouldBe(new DateTimeOffset(new DateTime(2013, 12, 6)));
            readReleaseNotes.Releases[1].ReleaseNoteLines.Count.ShouldBe(2);
            readReleaseNotes.Releases[1].ReleaseNoteItems[0].Title.ShouldBe("Issue 2 [#2](http://github.com/org/repo/issues/2) +feature");
            readReleaseNotes.Releases[1].ReleaseNoteItems[1].Title.ShouldBe("Issue 3 [#3](http://github.com/org/repo/issues/3) +fix");
        }
    }
}