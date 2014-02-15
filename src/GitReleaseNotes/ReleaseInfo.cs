using System;

namespace GitReleaseNotes
{
    public class ReleaseInfo
    {
        public ReleaseInfo()
        {
        }

        public ReleaseInfo(string name, DateTimeOffset? when, DateTimeOffset? previousReleaseDate)
        {
            Name = name;
            When = when;
            PreviousReleaseDate = previousReleaseDate;
        }

        public string Name { get; set; }
        public DateTimeOffset? When { get; set; }
        public DateTimeOffset? PreviousReleaseDate { get; set; }
    }
}