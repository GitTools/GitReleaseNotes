using LibGit2Sharp;

namespace GitReleaseNotes.Tests.IssueTrackers.GitHub
{
    public class NetworkEx : Network
    {
        private readonly RemoteCollection _remotes;

        public NetworkEx()
        {
            _remotes = new RemoteCollectionEx();
        }

        public override RemoteCollection Remotes
        {
            get { return _remotes; }
        }
    }
}