using System;
using System.Collections.Generic;

namespace GitReleaseNotes.IssueTrackers
{
    public interface IIssueTracker
    {
        bool VerifyArgumentsAndWriteErrorsToConsole();
        void PublishRelease(string releaseNotesOutput);
        IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since);
        bool RemotePresentWhichMatches { get; }
    }
}