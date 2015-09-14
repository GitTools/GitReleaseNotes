using GitTools;
using GitTools.IssueTrackers;

namespace GitReleaseNotes
{
    public class IssueTrackerParameters
    {
        public IssueTrackerParameters()
        {
            Authentication = new AuthenticationContext();
        }

        public string Server { get; set; }
        public string ProjectId { get; set; }
        public IssueTrackerType? Type { get; set; }
        public AuthenticationContext Authentication { get; private set; }
    }
}