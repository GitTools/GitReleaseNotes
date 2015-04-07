using System;
using System.Diagnostics;

namespace GitReleaseNotes
{
    internal class ConsoleLog : ILog
    {
        public void WriteLine(string s)
        {
            Console.WriteLine(s);

            Debug.WriteLine(s);
        }
    }
}