using System;

namespace GitReleaseNotes
{
    public class ReleaseNoteItem
    {
        private readonly string _title;
        private readonly string _issueNumber;
        private readonly Uri _htmlUrl;
        private readonly string[] _tags;
        private readonly DateTimeOffset? _resolvedOn;
        private Contributor[] _contributors;

        public ReleaseNoteItem(string title, string issueNumber, Uri htmlUrl, string[] tags, DateTimeOffset? resolvedOn, Contributor[] contributors)
        {
            _title = title;
            _issueNumber = issueNumber;
            _htmlUrl = htmlUrl;
            _tags = tags ?? new string[0];
            _resolvedOn = resolvedOn;
            _contributors = contributors;
        }

        public string Title
        {
            get { return _title; }
        }

        public Uri HtmlUrl
        {
            get { return _htmlUrl; }
        }

        public string[] Tags
        {
            get { return _tags; }
        }

        public string IssueNumber
        {
            get { return _issueNumber; }
        }

        public DateTimeOffset? ResolvedOn
        {
            get { return _resolvedOn; }
        }

        public Contributor[] Contributors { get { return _contributors; }}
    }
}