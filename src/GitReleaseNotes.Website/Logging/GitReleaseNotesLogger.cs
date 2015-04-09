namespace GitReleaseNotes.Website.Logging
{
    using Catel.Logging;

    public class GitReleaseNotesLogger : GitReleaseNotes.ILog
    {
        public static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public void WriteLine(string s)
        {
            Log.Write(LogEvent.Info, s);
        }
    }
}
