using System;
using System.Collections.Generic;

namespace GitReleaseNotes.IssueTrackers
{
    public interface IIssueTracker
    {
        bool VerifyArgumentsAndWriteErrorsToLog();
        IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since);
        bool RemotePresentWhichMatches { get; }
        string DiffUrlFormat { get; }
    }
}