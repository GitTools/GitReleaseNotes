using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GitReleaseNotes.IssueTrackers
{
    public interface IIssueTracker
    {
        bool VerifyArgumentsAndWriteErrorsToConsole();
        void PublishRelease(string releaseNotesOutput);
        IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since);
        Regex IssueNumberRegex { get; }
        bool RemotePresentWhichMatches { get; }
    }
}