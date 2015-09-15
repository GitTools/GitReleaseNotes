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

        public string WorkingDirectory { get; set; }
    }
}