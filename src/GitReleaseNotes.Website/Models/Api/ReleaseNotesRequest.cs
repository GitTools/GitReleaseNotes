namespace GitReleaseNotes.Website.Models.Api
{
    public class ReleaseNotesRequest
    {
        public string RepositoryUrl { get; set; }

        public string RepositoryBranch { get; set; }

        public string IssueTrackerUrl { get; set; }

        public string IssueTrackerProjectId { get; set; }
    }
}