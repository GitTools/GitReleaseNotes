using System;
using GitReleaseNotes.IssueTrackers.GitHub;

namespace GitReleaseNotes
{
    public class SemanticRelease
    {
        public SemanticRelease(string releaseName, DateTimeOffset? when, ReleaseNoteItem[] releaseNoteItems)
        {
            ReleaseName = releaseName;
            When = when;
            ReleaseNoteItems = releaseNoteItems;
        }

        public string ReleaseName { get; set; }
        public DateTimeOffset? When { get; set; }
        public ReleaseNoteItem[] ReleaseNoteItems { get; private set; }
    }
}