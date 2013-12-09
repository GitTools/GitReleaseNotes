using System;

namespace GitReleaseNotes
{
    public class ArgumentVerifier
    {
        public static bool VerifyArguments(GitReleaseNotesArguments arguments)
        {
            if (arguments.IssueTracker == null)
            {
                Console.WriteLine("The IssueTracker argument must be provided, see help (/?) for possible options");
                {
                    return false;
                }
            }
            if (string.IsNullOrEmpty(arguments.OutputFile) || !arguments.OutputFile.EndsWith(".md"))
            {
                Console.WriteLine("Specify an output file (*.md) [/OutputFile ...]");
                {
                    return false;
                }
            }
            return true;
        }
    }
}