using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace GitReleaseNotes.IssueTrackers
{
    public class IssueNumberExtractor : IIssueNumberExtractor
    {
        /// <param name="issueNumberRegexPattern">Must have a named group called 'issueNumber'</param>
        public List<string> GetIssueNumbers(GitReleaseNotesArguments arguments, Commit[] commitsToScan, string issueNumberRegexPattern)
        {
            var issueNumbersToScan = new List<string>();
            var issueRegex = new Regex(issueNumberRegexPattern, RegexOptions.Compiled);

            foreach (var commit in commitsToScan)
            {
                var matches = issueRegex.Matches(commit.Message).Cast<Match>();

                foreach (var match in matches)
                {
                    var issueNumber = match.Groups["issueNumber"].Value;
                    if (arguments.Verbose)
                        Console.WriteLine("Found issues {0} in commit {1}", issueNumber, commit.Sha);
                    issueNumbersToScan.Add(issueNumber);
                }
            }
            return issueNumbersToScan;
        }
    }
}