using System;

namespace GitReleaseNotes
{
    public class GitReleaseNotesException : Exception
    {
        public GitReleaseNotesException(string messageFormat, params object[] args)
            : this(string.Format(messageFormat, args))
        {
        }

        public GitReleaseNotesException(string message)
            : base(message)
        {
            
        }
    }
}