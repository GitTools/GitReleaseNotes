using System;
using System.Collections.Generic;

namespace GitReleaseNotes.IssueTrackers.YouTrack
{
    public interface IYouTrackApi
    {
        IEnumerable<OnlineIssue> GetClosedIssues(GitReleaseNotesArguments arguments, DateTimeOffset? since);
    }
}