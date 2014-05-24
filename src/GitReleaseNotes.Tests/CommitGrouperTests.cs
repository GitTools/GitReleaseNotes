using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GitReleaseNotes.Git;
using LibGit2Sharp;
using NSubstitute;
using Shouldly;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class CommitGrouperTests
    {
        private readonly Dictionary<Commit, string> _tags;
        private readonly IRepository _repository;
        private readonly CommitGrouper _sut;
        private readonly Random _random;
        private DateTimeOffset _nextCommitDate;

        public CommitGrouperTests()
        {
            _nextCommitDate = DateTimeOffset.Now;
            _repository = Substitute.For<IRepository>();
            _tags = new Dictionary<Commit, string>();
            var tagCollection = Substitute.For<TagCollection>();
            tagCollection.GetEnumerator().Returns(c => _tags.Select(p =>
            {
                var tag = Substitute.For<Tag>();
                tag.Target.Returns(p.Key);
                tag.Name.Returns(p.Value);
                return tag;
            }).GetEnumerator());
            _repository.Tags.Returns(tagCollection);
            _random = new Random();
            _sut = new CommitGrouper();
        }

        [Fact]
        public void DoesNotIncludeCommitsOlderThanTag()
        {
            var commit1 = CreateCommit();
            var startTagCommit = CreateCommit();
            var commit3 = CreateCommit();
            SubstituteCommitLog(commit1, startTagCommit, commit3);
            var startTag = new TaggedCommit(startTagCommit, "1.0.0");

            var results = _sut.GetCommitsByRelease(_repository, startTag);

            var firstRelease = results.First();
            firstRelease.Value.Count.ShouldBe(1);
            firstRelease.Key.PreviousReleaseDate.ShouldBe(startTagCommit.Author.When);
        }

        [Fact]
        public void GroupsTagsByReleases()
        {
            var commit1 = CreateCommit();
            var commit2 = CreateCommit();
            var commit3 = CreateCommit();
            var startTagCommit = CreateCommit();
            SubstituteCommitLog(commit1, commit2, commit3, startTagCommit);
            _tags.Add(commit2, "1.1.0");
            var startTag = new TaggedCommit(startTagCommit, "1.0.0");

            var results = _sut.GetCommitsByRelease(_repository, startTag);

            results.Count.ShouldBe(2);
            results.ElementAt(0).Key.Name.ShouldBe(null);
            results.ElementAt(0).Value.Count.ShouldBe(1);
            results.ElementAt(0).Key.PreviousReleaseDate.ShouldBe(commit2.Author.When);
            results.ElementAt(1).Key.Name.ShouldBe("1.1.0");
            results.ElementAt(1).Key.PreviousReleaseDate.ShouldBe(null);
            results.ElementAt(1).Value.Count.ShouldBe(2);
            results.ElementAt(1).Value.ElementAt(0).ShouldBe(commit2);
        }

        [Fact]
        public void GroupsTagsByReleasesIncludesEndDateOfRelease()
        {
            var commit1 = CreateCommit();
            var commit2 = CreateCommit();
            var commit3 = CreateCommit();
            var startTagCommit = CreateCommit();
            var firstCommit = CreateCommit();
            SubstituteCommitLog(commit1, commit2, commit3, startTagCommit, firstCommit);
            _tags.Add(commit2, "1.1.0");
            _tags.Add(startTagCommit, "1.0.0");

            var results = _sut.GetCommitsByRelease(_repository, null);

            Assert.Equal(3, results.Count);
            Assert.Equal(null, results.ElementAt(0).Key.Name);
            Assert.Equal(null, results.ElementAt(0).Key.When);
            Assert.Equal(commit2.Author.When, results.ElementAt(0).Key.PreviousReleaseDate);
            Assert.Equal(commit2.Author.When, results.ElementAt(1).Key.When);
            Assert.Equal(startTagCommit.Author.When, results.ElementAt(1).Key.PreviousReleaseDate);
            Assert.Equal("1.0.0", results.ElementAt(2).Key.Name);
            Assert.Equal(startTagCommit.Author.When, results.ElementAt(2).Key.When);
            Assert.Equal(null, results.ElementAt(2).Key.PreviousReleaseDate);
        }

        private Commit CreateCommit()
        {
            var commit = Substitute.For<Commit>();
            commit.Author.Returns(new Signature("Some Dude", "some@dude.com", _nextCommitDate));
            _nextCommitDate = _nextCommitDate.AddHours(-1);
            var random = _random.Next().ToString(CultureInfo.InvariantCulture);
            var randomSha1 = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(random));
            commit.Id.Returns(new ObjectId(randomSha1));
            commit.Sha.Returns(BitConverter.ToString(randomSha1).Replace("-", string.Empty));
            return commit;
        }

        private void SubstituteCommitLog(params Commit[] commits)
        {
            var commitLog = Substitute.For<IQueryableCommitLog>();
            var returnThis = commits.AsEnumerable().GetEnumerator();
            commitLog.GetEnumerator().Returns(returnThis);
            _repository.Commits.Returns(commitLog);
        }
    }
}