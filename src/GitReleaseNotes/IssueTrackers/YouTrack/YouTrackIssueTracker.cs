using System;
using System.Collections.Generic;
using System.Linq;

namespace GitReleaseNotes.IssueTrackers.YouTrack
{
    public sealed class YouTrackIssueTracker : IIssueTracker
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        private readonly Context _context;
        private readonly IYouTrackApi _youTrackApi;

        public YouTrackIssueTracker(IYouTrackApi youTrackApi, Context context)
        {
            _youTrackApi = youTrackApi;
            _context = context;

            var youTrackContext = (YouTrackContext) context.IssueTracker;
            if (string.IsNullOrWhiteSpace(youTrackContext.Filter))
            {
                youTrackContext.Filter = string.Format(
                    "project:{0} State:Resolved State:-{{Won't fix}} State:-{{Can't Reproduce}} State:-Duplicate", 
                    youTrackContext.ProjectId);
            }
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(IIssueTrackerContext context, DateTimeOffset? since)
        {
            return _youTrackApi.GetClosedIssues(context, since).ToArray();
        }
    }
}
