using LibGit2Sharp;

namespace GitReleaseNotes.Tests.IssueTrackers.GitHub
{
    public class RemoteEx : Remote
    {
        private readonly string _url;
        private readonly string _name;

        public override string Name
        {
            get { return _name; }
        }

        public override string Url
        {
            get { return _url; }
        }

        public RemoteEx(string name, string url)
        {
            _name = name;
            _url = url;
        }
    }
}