using System;

namespace GitReleaseNotes.IssueTrackers
{
    public class OnlineIssue
    {
        public OnlineIssue(string id, DateTimeOffset dateClosed)
        {
            Id = id;
            DateClosed = dateClosed;
        }

        public string Id { get; private set; }
        public DateTimeOffset DateClosed { get; set; }

        public IssueType IssueType { get; set; }
        public Uri HtmlUrl { get; set; }
        public string Title { get; set; }
        public string[] Labels { get; set; }
        public Contributor[] Contributors { get; set; }
    }
}