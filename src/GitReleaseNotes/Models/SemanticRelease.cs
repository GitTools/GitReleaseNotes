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

        public SemanticRelease(string releaseName, DateTimeOffset? when, ReleaseDiffInfo diffInfo = null, IEnumerable<IReleaseNoteLine> releaseNoteLines = null)
            : this()
        {
            ReleaseName = releaseName;
            When = when;

            if (diffInfo != null)
            {
                DiffInfo = diffInfo;
            }

            if (releaseNoteLines != null)
            {
                ReleaseNoteLines.AddRange(releaseNoteLines);
            }
        }

        public string ReleaseName { get; set; }
        public DateTimeOffset? When { get; set; }
        public List<IReleaseNoteLine> ReleaseNoteLines { get; private set; }
        public ReleaseNoteItem[] ReleaseNoteItems { get { return ReleaseNoteLines.OfType<ReleaseNoteItem>().ToArray(); }}
        public ReleaseDiffInfo DiffInfo { get; private set; }
    }
}