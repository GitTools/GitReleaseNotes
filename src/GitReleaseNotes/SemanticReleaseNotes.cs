using System.Collections.Generic;
using System.Linq;

namespace GitReleaseNotes
{
    public class SemanticReleaseNotes
    {
        private readonly SemanticRelease[] _releases;

        public SemanticReleaseNotes(IEnumerable<SemanticRelease> releaseNoteItems)
        {
            _releases = releaseNoteItems.ToArray();
        }

        public SemanticRelease[] Releases
        {
            get { return _releases; }
        }
    }
}