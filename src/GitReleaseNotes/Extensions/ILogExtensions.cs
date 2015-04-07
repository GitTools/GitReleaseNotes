namespace GitReleaseNotes
{
    public static class ILogExtensions
    {
        public static void WriteLine(this ILog log, string format, params object[] args)
        {
            log.WriteLine(string.Format(format, args));
        }
    }
}