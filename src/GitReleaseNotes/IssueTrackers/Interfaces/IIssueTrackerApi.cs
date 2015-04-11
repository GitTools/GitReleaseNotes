namespace GitReleaseNotes.IssueTrackers
{
    using System;
    using System.Collections.Generic;

    public interface IIssueTrackerApi
    {
        IEnumerable<OnlineIssue> GetClosedIssues(IIssueTrackerContext context, DateTimeOffset? since);
    }
}
