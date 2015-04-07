namespace GitReleaseNotes
{
    public static class GitReleaseNotesEnvironment
    {
        #region Fields

        public static ILog Log = new CustomLog(s => { });

        #endregion
    }
}