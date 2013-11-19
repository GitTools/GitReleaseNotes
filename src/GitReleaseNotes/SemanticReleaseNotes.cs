using System.Collections.Generic;
using System.Linq;
using GitReleaseNotes.IssueTrackers.GitHub;

namespace GitReleaseNotes
{
    public class SemanticReleaseNotes
    {
        private readonly ReleaseNoteItem[] _releaseNoteItems;

        public SemanticReleaseNotes(IEnumerable<ReleaseNoteItem> releaseNoteItems)
        {
            _releaseNoteItems = releaseNoteItems.ToArray();
        }

        public ReleaseNoteItem[] ReleaseNoteItems
        {
            get { return _releaseNoteItems; }
        }
    }
}