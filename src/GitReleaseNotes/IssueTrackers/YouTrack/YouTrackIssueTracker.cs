using System;
using System.Collections.Generic;
using System.Linq;

namespace GitReleaseNotes.IssueTrackers.YouTrack
{
    public sealed class YouTrackIssueTracker : IIssueTracker
    {
        private readonly GitReleaseNotesArguments arguments;
        private readonly IYouTrackApi youTrackApi;
        private readonly ILog log;

        public YouTrackIssueTracker(IYouTrackApi youTrackApi, ILog log, GitReleaseNotesArguments arguments)
        {
            this.youTrackApi = youTrackApi;
            this.log = log;
            this.arguments = arguments;
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole()
        {
            if (string.IsNullOrEmpty(arguments.YouTrackServer) ||
                !Uri.IsWellFormedUriString(arguments.YouTrackServer, UriKind.Absolute))
            {
                log.WriteLine("A valid YouTrack server must be specified [/YouTrackServer ]");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.ProjectId))
            {
                log.WriteLine("/ProjectId is a required parameter for YouTrack");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.Username))
            {
                log.WriteLine("/Username is a required to authenticate with YouTrack");
                return false;
            }
            if (string.IsNullOrEmpty(arguments.Password))
            {
                log.WriteLine("/Password is a required to authenticate with YouTrack");
                return false;
            }

            if (string.IsNullOrEmpty(arguments.YouTrackFilter))
            {
                arguments.YouTrackFilter = string.Format(
                    "project:{0} State:Resolved State:-{{Won't fix}} State:-{{Can't Reproduce}} State:-Duplicate", 
                    arguments.ProjectId);
            }

            return true;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            return youTrackApi.GetClosedIssues(arguments, since).ToArray();
        }

        public bool RemotePresentWhichMatches
        {
            get
            {
                return false;
            }
        }

        public string DiffUrlFormat { get { return string.Empty; }}
    }
}
