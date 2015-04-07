namespace GitReleaseNotes
{
    public class ArgumentVerifier
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        public static bool VerifyArguments(GitReleaseNotesArguments arguments)
        {
            if (string.IsNullOrEmpty(arguments.OutputFile))
            {
                Log.WriteLine("WARN: No Output file specified (*.md) [/OutputFile ...]");
            }

            if (!string.IsNullOrEmpty(arguments.OutputFile) && !arguments.OutputFile.EndsWith(".md"))
            {
                Log.WriteLine("WARN: Output file should have a .md extension [/OutputFile ...]");
                arguments.OutputFile = null;
            }

            return true;
        }
    }
}