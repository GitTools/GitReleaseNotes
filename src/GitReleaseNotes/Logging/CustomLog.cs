using System;
using System.Diagnostics;

namespace GitReleaseNotes
{
    public class CustomLog : ILog
    {
        private readonly Action<string> logAction;

        public CustomLog(Action<string> logAction)
        {
            this.logAction = logAction;
        }

        public void WriteLine(string s)
        {
            if (logAction != null)
            {
                logAction(s);
            }

            Debug.WriteLine(s);
        }
    }
}