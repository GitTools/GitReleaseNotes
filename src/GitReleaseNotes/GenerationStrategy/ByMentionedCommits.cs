using System;
using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.IssueTrackers.GitHub;
using LibGit2Sharp;

namespace GitReleaseNotes.GenerationStrategy
{
    public class ByMentionedCommits : IReleaseNotesStrategy
    {
        private readonly IIssueNumberExtractor _issueNumberExtractor;

        public ByMentionedCommits(IIssueNumberExtractor issueNumberExtractor)
        {
            _issueNumberExtractor = issueNumberExtractor;
        }

        public SemanticReleaseNotes GetReleaseNotes(Dictionary<ReleaseInfo, List<Commit>> releases, GitReleaseNotesArguments arguments, IIssueTracker issueTracker)
        {
            Console.WriteLine("Scanning {0} commits over {1} releases for issue numbers", releases.Sum(r => r.Value.Count),
                releases.Count);

            if (arguments.Verbose)
            {
                foreach (var release in releases)
                {
                    foreach (var commit in release.Value)
                    {
                        Console.WriteLine("[{0}] {1}", release.Key.Name, commit.Message);
                    }
                }
            }

            var issueNumbersToScan = _issueNumberExtractor.GetIssueNumbers(arguments, releases, issueTracker.IssueNumberRegex);

            var since = releases.Select(c => c.Key.When).Min();
            var potentialIssues = issueTracker.GetClosedIssues(since).Where(i => i.IssueType != IssueType.PullRequest).ToArray();

            var closedMentionedIssuesByRelease = issueNumbersToScan.Select(issues =>
            {
                var issuesForRelease = potentialIssues
                    .Where(i => issues.Value.Contains(i.Id))
                    .ToArray();
                return new
                {
                    ReleaseInfo = issues.Key,
                    IssuesForRelease = issuesForRelease
                };
            })
                .Where(g => g.IssuesForRelease.Any())
                .OrderBy(g => g.ReleaseInfo.When);

            return new SemanticReleaseNotes(closedMentionedIssuesByRelease.Select(r =>
            {
                var releaseNoteItems = r.IssuesForRelease.Select(i =>
                {
                    var labels = i.Labels ?? new string[0];
                    return new ReleaseNoteItem(i.Title, string.Format("{0}", i.Id), i.HtmlUrl, labels);
                }).ToArray();
                return new SemanticRelease(r.ReleaseInfo.Name, r.ReleaseInfo.When, releaseNoteItems);
            }));
        }
    }
}