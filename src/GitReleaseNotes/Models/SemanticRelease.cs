using System;
using System.Collections.Generic;
using System.Linq;

namespace GitReleaseNotes
{
    public class SemanticRelease
    {
        public SemanticRelease()
        {
            DiffInfo = new ReleaseDiffInfo();
            ReleaseNoteLines = new List<IReleaseNoteLine>();
        }

        public SemanticRelease(string releaseName, DateTimeOffset? when, List<IReleaseNoteLine> releaseNoteLines, ReleaseDiffInfo diffInfo)
        {
            DiffInfo = diffInfo;
            ReleaseName = releaseName;
            When = when;
            ReleaseNoteLines = releaseNoteLines;
        }

        public string ReleaseName { get; set; }
        public DateTimeOffset? When { get; set; }
        public List<IReleaseNoteLine> ReleaseNoteLines { get; private set; }
        public ReleaseNoteItem[] ReleaseNoteItems { get { return ReleaseNoteLines.OfType<ReleaseNoteItem>().ToArray(); }}
        public ReleaseDiffInfo DiffInfo { get; private set; }
    }
}