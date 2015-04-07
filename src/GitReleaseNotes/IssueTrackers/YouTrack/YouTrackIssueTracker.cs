using System;
using System.Collections.Generic;
using System.Linq;

namespace GitReleaseNotes.IssueTrackers.YouTrack
{
    public sealed class YouTrackIssueTracker : IIssueTracker
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly Context context;
        private readonly IYouTrackApi youTrackApi;

        public YouTrackIssueTracker(IYouTrackApi youTrackApi, Context context)
        {
            this.youTrackApi = youTrackApi;
            this.context = context;
        }

        public bool VerifyArgumentsAndWriteErrorsToLog()
        {
            if (string.IsNullOrEmpty(context.YouTrack.YouTrackServer) ||
                !Uri.IsWellFormedUriString(context.YouTrack.YouTrackServer, UriKind.Absolute))
            {
                Log.WriteLine("A valid YouTrack server must be specified [/YouTrackServer ]");
                return false;
            }

            if (string.IsNullOrEmpty(context.ProjectId))
            {
                Log.WriteLine("/ProjectId is a required parameter for YouTrack");
                return false;
            }

            if (string.IsNullOrEmpty(context.Authentication.Username))
            {
                Log.WriteLine("/Username is a required to authenticate with YouTrack");
                return false;
            }
            if (string.IsNullOrEmpty(context.Authentication.Password))
            {
                Log.WriteLine("/Password is a required to authenticate with YouTrack");
                return false;
            }

            if (string.IsNullOrEmpty(context.YouTrack.YouTrackFilter))
            {
                context.YouTrack.YouTrackFilter = string.Format(
                    "project:{0} State:Resolved State:-{{Won't fix}} State:-{{Can't Reproduce}} State:-Duplicate", 
                    context.ProjectId);
            }

            return true;
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            return youTrackApi.GetClosedIssues(context, since).ToArray();
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
