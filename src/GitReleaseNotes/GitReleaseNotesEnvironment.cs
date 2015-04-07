namespace GitReleaseNotes
{
    public static class GitReleaseNotesEnvironment
    {
        public static ILog Log = new CustomLog(s => { });
    }
}