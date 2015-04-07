using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GitReleaseNotes.IssueTrackers;
using LibGit2Sharp;
using NSubstitute;

namespace GitReleaseNotes.Tests
{
    public class TestDataCreator
    {
        private readonly DateTimeOffset _initialCommitTime;
        private readonly List<Tuple<string, OnlineIssue[]>> _releases = new List<Tuple<string, OnlineIssue[]>>();
        private int _idCounter;
        private readonly List<string> _additionalIssues = new List<string>();

        public TestDataCreator(DateTimeOffset initialCommitTime)
        {
            _initialCommitTime = initialCommitTime;
        }

        public TestDataCreator CreateRelease(string tag, params string[] issues)
        {
            _releases.Add(Tuple.Create(tag, issues.Select(i => new OnlineIssue(GetNextId(), DateTime.Now)
            {
                IssueType = IssueType.Issue,
                Title = i
            }).ToArray()));

            return this;
        }

        public TestDataCreator AddIssues(params string[] issues)
        {
            _additionalIssues.AddRange(issues);
            return this;
        }

        public void Build(out IRepository repo, out IIssueTracker issueTracker)
        {
            var commits = new List<Commit>();
            var tags = new List<Tag>();
            var closedIssues = new List<OnlineIssue>();
            repo = Substitute.For<IRepository>();
            issueTracker = Substitute.For<IIssueTracker>();

            commits.Add(CreateCommit(_initialCommitTime));
            var currentDate = _initialCommitTime.AddDays(1);

            foreach (var release in _releases)
            {
                // Create a commit, which *fixes* all the closed issues in the release
                commits.Add(CreateCommit(currentDate));

                // Create closed issues
                foreach (var issue in release.Item2)
                {
                    issue.DateClosed = currentDate;
                    closedIssues.Add(issue);
                }

                // Create commit which completes the release, and tag that commit
                currentDate = currentDate.AddDays(1);
                var commit = CreateCommit(currentDate);
                commits.Add(commit);
                tags.Add(CreateTag(commit, release.Item1));
                currentDate = currentDate.AddDays(1);
            }
            foreach (var additionalIssue in _additionalIssues)
            {
                closedIssues.Add(new OnlineIssue(GetNextId(), currentDate)
                {
                    Title = additionalIssue,
                    IssueType = IssueType.Issue
                });
                var commit = CreateCommit(currentDate);
                commits.Add(commit);
            }

            SubstituteCommitLog(repo, commits, tags);
            issueTracker
                .GetClosedIssues(Arg.Any<DateTimeOffset?>())
                .Returns(c => closedIssues.Where(i => c.Arg<DateTimeOffset?>() == null || i.DateClosed > c.Arg<DateTimeOffset?>()));
        }

        private string GetNextId()
        {
            return (_idCounter++).ToString(CultureInfo.InvariantCulture);
        }

        private Commit CreateCommit(DateTimeOffset when)
        {
            var commit = Substitute.For<Commit>();
            var formattedDate = when.DateTime.ToString("F", CultureInfo.GetCultureInfo("en-US"));
            var shaBasedOnDate = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(formattedDate));
            commit.Author.Returns(new Signature("Some Dude", "some@dude.com", when));
            commit.Id.Returns(new ObjectId(shaBasedOnDate));
            commit.Sha.Returns(BitConverter.ToString(shaBasedOnDate).Replace("-", string.Empty));
            return commit;
        }

        private Tag CreateTag(GitObject commit, string name)
        {
            var tag = Substitute.For<Tag>();
            tag.Target.Returns(commit);
            tag.Name.Returns(name);
            tag.CanonicalName.Returns(name);
            return tag;
        }

        private void SubstituteCommitLog(IRepository repo, List<Commit> commits, IEnumerable<Tag> tags)
        {
            var commitLog = Substitute.For<IQueryableCommitLog>();
            var tagCollection = Substitute.For<TagCollection>();
            commits.Reverse();
            commitLog.GetEnumerator().Returns(c => commits.GetEnumerator());
            tagCollection.GetEnumerator().Returns(c => tags.GetEnumerator());

            repo.Commits.Returns(commitLog);
            repo.Tags.Returns(tagCollection);
            var branch = Substitute.For<Branch>();
            branch.Commits.Returns(commitLog);
            branch.Tip.Returns(commits.First());
            repo.Head.Returns(branch);
        }
    }
}