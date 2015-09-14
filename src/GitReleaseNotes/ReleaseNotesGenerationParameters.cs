using System;
using GitTools;

namespace GitReleaseNotes
{
    public class ReleaseNotesGenerationParameters
    {
        public ReleaseNotesGenerationParameters()
        {
            RepositorySettings = new RepositoryContext();
            IssueTracker = new IssueTrackerParameters();
        }

        public string Categories { get; set; }
        public string Version { get; set; }

        public bool AllTags { get; set; }
        public bool AllLabels { get; set; }
        public RepositoryContext RepositorySettings { get; private set; }
        public IssueTrackerParameters IssueTracker { get; private set; }

        [Obsolete("Release notes generation should have nothing to do with an output file")]
        public string OutputFile { get; set; }

        public string WorkingDirectory { get; set; }
    }
}