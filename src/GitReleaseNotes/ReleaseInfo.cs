using System;

namespace GitReleaseNotes
{
    public class ReleaseInfo
    {
        public ReleaseInfo()
        {
        }

        public ReleaseInfo(string name, DateTimeOffset? when)
        {
            Name = name;
            When = when;
        }

        public string Name { get; set; }
        public DateTimeOffset? When { get; set; }
    }
}