using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitReleaseNotes.Tests.IssueTrackers.GitHub
{
    public class RemoteCollectionEx : RemoteCollection
    {
        readonly List<Remote> _remotes = new List<Remote>();

        public override Remote Add(string name, string url)
        {
            var remoteEx = new RemoteEx(name, url);
            _remotes.Add(remoteEx);
            return remoteEx;
        }

        public override IEnumerator<Remote> GetEnumerator()
        {
            return _remotes.GetEnumerator();
        }

        public override Remote this[string name]
        {
            get { return _remotes.Single(r => r.Name == name); }
        }
    }
}