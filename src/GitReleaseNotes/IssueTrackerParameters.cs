using GitTools;
using GitTools.Git;
using GitTools.IssueTrackers;

namespace GitReleaseNotes
{
    public class IssueTrackerParameters
    {
        public IssueTrackerParameters()
        {
            Authentication = new AuthenticationInfo();
        }

        public string Server { get; set; }
        public string ProjectId { get; set; }
        public IssueTrackerType? Type { get; set; }
        public AuthenticationInfo Authentication { get; private set; }
    }
}