using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitReleaseNotes.IssueTrackers.YouTrack
{
    public sealed class YouTrackIssueTracker : IIssueTracker
    {
        private readonly GitReleaseNotesArguments _arguments;
        private readonly Regex _issueNumberRegex;
        private readonly IYouTrackApi _youTrackApi;

        public YouTrackIssueTracker(IYouTrackApi youTrackApi, GitReleaseNotesArguments arguments)
        {
            _youTrackApi = youTrackApi;
            _arguments = arguments;
            _issueNumberRegex = new Regex(string.Format(@"(?<issueNumber>{0}-\d+)", arguments.ProjectId));
        }

        public bool VerifyArgumentsAndWriteErrorsToConsole()
        {
            if (string.IsNullOrEmpty(_arguments.YouTrackServer) ||
                !Uri.IsWellFormedUriString(_arguments.YouTrackServer, UriKind.Absolute))
            {
                Console.WriteLine("A valid YouTrack server must be specified [/YouTrackServer ]");
                return false;
            }

            if (string.IsNullOrEmpty(_arguments.ProjectId))
            {
                Console.WriteLine("/ProjectId is a required parameter for YouTrack");
                return false;
            }

            if (string.IsNullOrEmpty(_arguments.Username))
            {
                Console.WriteLine("/Username is a required to authenticate with YouTrack");
                return false;
            }
            if (string.IsNullOrEmpty(_arguments.Password))
            {
                Console.WriteLine("/Password is a required to authenticate with YouTrack");
                return false;
            }

            if (string.IsNullOrEmpty(_arguments.YouTrackFilter))
            {
                _arguments.YouTrackFilter = string.Format(
                    "project:{0} State:Resolved State:-{{Won't fix}} State:-{{Can't Reproduce}} State:-Duplicate", 
                    _arguments.ProjectId);
            }

            return true;
        }

        public void PublishRelease(string releaseNotesOutput)
        {
            Console.WriteLine("YouTrack does not support publishing releases");
        }

        public IEnumerable<OnlineIssue> GetClosedIssues(DateTimeOffset? since)
        {
            return _youTrackApi.GetClosedIssues(_arguments, since).ToArray();
        }

        public Regex IssueNumberRegex
        {
            get
            {
                return _issueNumberRegex;
            }
        }

        public bool RemotePresentWhichMatches
        {
            get
            {
                return false;
            }
        }
    }
}
