using System;
using System.Collections.Generic;

namespace GitReleaseNotes.IssueTrackers.YouTrack
{
    public interface IYouTrackApi
    {
        IEnumerable<OnlineIssue> GetClosedIssues(Context context, DateTimeOffset? since);
    }
}