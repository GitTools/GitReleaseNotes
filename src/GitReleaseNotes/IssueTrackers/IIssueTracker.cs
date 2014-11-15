using System;
using System.Collections.Generic;

namespace GitReleaseNotes.IssueTrackers
{
    public interface IIssueTracker
    {
        bool VerifyArgumentsAndWriteErrorsToConsole();
        IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since);
        bool RemotePresentWhichMatches { get; }
        string DiffUrlFormat { get; }
    }
}