using System;
using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

namespace GitReleaseNotes.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class ReleaseNotesGeneratorTests
    {
        private readonly ReleaseNotesGenerator _sut;

        public ReleaseNotesGeneratorTests()
        {
            _sut = new ReleaseNotesGenerator();
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
                new SemanticRelease("", null, new List<ReleaseNoteItem>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new string[0], DateTime.Now, new[]{ new Contributor("Foo Bar", "@foo", "http://url.com/foo") }),
                    new ReleaseNoteItem("Issue 1", null, null, new string[0], DateTime.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            });

            var result =_sut.GenerateReleaseNotes(arguments, releaseNotes, new SemanticReleaseNotes());

            Approvals.Verify(result);
        }

        [Fact]
        public void ApproveSimpleUpdateTests()
        {
            var arguments = new GitReleaseNotesArguments
            {
                OutputFile = "ReleaseFile.md"
            };
            var releaseNotes = new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", null, new List<ReleaseNoteItem>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new string[0], DateTime.Now.AddDays(-2), new Contributor[0]),
                    new ReleaseNoteItem("Issue 2", "#2", new Uri("http://github.com/org/repo/issues/2"), new string[0], DateTime.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            });

            var previousReleaseNotesGeneratedOn = DateTime.Now.AddDays(-1);
            var result =_sut.GenerateReleaseNotes(arguments, releaseNotes, new SemanticReleaseNotes(new[]
            {
                new SemanticRelease("", previousReleaseNotesGeneratedOn, new List<ReleaseNoteItem>
                {
                    new ReleaseNoteItem("Customised title for Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new string[0], previousReleaseNotesGeneratedOn,
                        new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "asd39424",
                    EndSha = "134asd53dsa"
                }), 
            }));

            Approvals.Verify(result);
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
                new SemanticRelease("", null, new List<ReleaseNoteItem>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"feature"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            });

            var result = _sut.GenerateReleaseNotes(arguments, releaseNotes, new SemanticReleaseNotes());

            Approvals.Verify(result);
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
                new SemanticRelease("", null, new List<ReleaseNoteItem>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"feature"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                }),
                new SemanticRelease("1.2.0", new DateTimeOffset(2013, 12, 06, 0,0,0, new TimeSpan()), new List<ReleaseNoteItem>
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
            });

            var result = _sut.GenerateReleaseNotes(arguments, releaseNotes, new SemanticReleaseNotes());

            Approvals.Verify(result);
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
                new SemanticRelease("", null, new List<ReleaseNoteItem>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"), new[] {"bug"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            });

            var result = _sut.GenerateReleaseNotes(arguments, releaseNotes, new SemanticReleaseNotes());

            Approvals.Verify(result);
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
                new SemanticRelease("", null, new List<ReleaseNoteItem>
                {
                    new ReleaseNoteItem("Issue 1", "#1", new Uri("http://github.com/org/repo/issues/1"),
                        new[] {"internal refactoring"}, DateTimeOffset.Now, new Contributor[0])
                }, new ReleaseDiffInfo
                {
                    BeginningSha = "12345678",
                    EndSha = "67890123"
                })
            });

            var result = _sut.GenerateReleaseNotes(arguments, releaseNotes, new SemanticReleaseNotes());

            Approvals.Verify(result);
        }
    }
}