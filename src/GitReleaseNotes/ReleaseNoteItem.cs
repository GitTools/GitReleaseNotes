using System;

namespace GitReleaseNotes
{
    public class ReleaseNoteItem
    {
        private readonly string _title;
        private readonly string _issueNumber;
        private readonly Uri _htmlUrl;
        private readonly string[] _tags;

        public ReleaseNoteItem(string title, string issueNumber, Uri htmlUrl, string[] tags)
        {
            _title = title;
            _issueNumber = issueNumber;
            _htmlUrl = htmlUrl;
            _tags = tags;
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
    }
}