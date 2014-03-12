using System;

namespace GitReleaseNotes
{
    public class SemanticRelease
    {
        public SemanticRelease(string releaseName, DateTimeOffset? when, ReleaseNoteItem[] releaseNoteItems, ReleaseDiffInfo diffInfo)
        {
            DiffInfo = diffInfo;
            ReleaseName = releaseName;
            When = when;
            ReleaseNoteItems = releaseNoteItems;
        }

        public string ReleaseName { get; set; }
        public DateTimeOffset? When { get; set; }
        public ReleaseNoteItem[] ReleaseNoteItems { get; private set; }
        public ReleaseDiffInfo DiffInfo { get; set; }
    }
}