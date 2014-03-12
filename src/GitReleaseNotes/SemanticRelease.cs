using System;
using System.Collections.Generic;

namespace GitReleaseNotes
{
    public class SemanticRelease
    {
        public SemanticRelease()
        {
            DiffInfo = new ReleaseDiffInfo();
            ReleaseNoteItems = new List<ReleaseNoteItem>();
        }

        public SemanticRelease(string releaseName, DateTimeOffset? when, List<ReleaseNoteItem> releaseNoteItems, ReleaseDiffInfo diffInfo)
        {
            DiffInfo = diffInfo;
            ReleaseName = releaseName;
            When = when;
            ReleaseNoteItems = releaseNoteItems;
        }

        public string ReleaseName { get; set; }
        public DateTimeOffset? When { get; set; }
        public List<ReleaseNoteItem> ReleaseNoteItems { get; set; }
        public ReleaseDiffInfo DiffInfo { get; set; }
    }
}