using System;

namespace GitReleaseNotes
{
    public class ReleaseInfo
    {
        public ReleaseInfo()
        {
        }

        public ReleaseInfo(string name, DateTimeOffset? when, DateTimeOffset? previousReleaseDate, string firstCommit)
        {
            Name = name;
            When = when;
            PreviousReleaseDate = previousReleaseDate;
            FirstCommit = firstCommit;
        }

        public string Name { get; set; }
        public DateTimeOffset? When { get; set; }
        public DateTimeOffset? PreviousReleaseDate { get; set; }
        public string FirstCommit { get; set; }
        public string LastCommit { get; set; }
    }
}