using System;
using System.Collections.Generic;

namespace GitReleaseNotes.IssueTrackers.Jira
{
    public interface IJiraApi
    {
        IEnumerable<OnlineIssue> GetClosedIssues(Context context, DateTimeOffset? since);
    }
}