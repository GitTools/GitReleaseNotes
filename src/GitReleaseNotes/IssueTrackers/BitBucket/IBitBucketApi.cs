using System;
using System.Collections.Generic;

namespace GitReleaseNotes.IssueTrackers.BitBucket
{
    public interface IBitBucketApi
    {
        IEnumerable<OnlineIssue> GetClosedIssues(GitReleaseNotesArguments arguments, DateTimeOffset? since, string accountName, string repoSlug, bool oauth);
    }
}