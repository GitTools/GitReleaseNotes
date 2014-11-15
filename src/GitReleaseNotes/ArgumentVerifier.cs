using System;

namespace GitReleaseNotes
{
    public class ArgumentVerifier
    {
        public static bool VerifyArguments(GitReleaseNotesArguments arguments)
        {
            if (string.IsNullOrEmpty(arguments.OutputFile))
            {
                Console.WriteLine("WARN: No Output file specified (*.md) [/OutputFile ...]");
            }
            if (!string.IsNullOrEmpty(arguments.OutputFile) && !arguments.OutputFile.EndsWith(".md"))
            {
                Console.WriteLine("WARN: Output file should have a .md extension [/OutputFile ...]");
                arguments.OutputFile = null;
            }
            return true;
        }
    }
}