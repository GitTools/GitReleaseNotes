using System;
using System.Collections.Generic;

namespace GitReleaseNotes.IssueTrackers.BitBucket
{
    public interface IBitBucketApi
    {
        IEnumerable<OnlineIssue> GetClosedIssues(Context context, DateTimeOffset? since, string accountName, string repoSlug, bool oauth);
    }
}