using System;

namespace GitReleaseNotes
{
    internal class Log : ILog
    {
        public void WriteLine(string s)
        {
            Console.WriteLine(s);
        }
    }
}