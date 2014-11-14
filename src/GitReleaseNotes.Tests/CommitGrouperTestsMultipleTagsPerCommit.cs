using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LibGit2Sharp;
using NSubstitute;
using Xunit;

namespace GitReleaseNotes.Tests
{
    public class CommitGrouperTestsMultipleTagsPerCommit
    {
        private readonly Dictionary<Commit, string> _tags;
        private readonly IRepository _repository;
        private readonly Random _random;
        private DateTimeOffset _nextCommitDate;

        public CommitGrouperTestsMultipleTagsPerCommit()
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
        }


        [Fact]
        public void SupportCommitsWithMoreThanOneTag()
        {
            var commit1 = CreateCommit();
            var commit2 = CreateCommit();
            var commit3 = CreateCommit();
            var startTagCommit = CreateCommit();
            var firstCommit = CreateCommit();
            SubstituteCommitLog(commit1, commit2, commit3, startTagCommit, firstCommit);
            _tags.Add(commit2, "1.1.0");
            _tags.Add(commit2, "second_tag_for_commit2");

            var results = ReleaseFinder.FindReleases(_repository, null, new ReleaseInfo());



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