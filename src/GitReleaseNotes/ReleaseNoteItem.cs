using System;
using System.Linq;

namespace GitReleaseNotes
{
    public class ReleaseNoteItem : IReleaseNoteLine
    {
        private readonly string title;
        private readonly string issueNumber;
        private readonly Uri htmlUrl;
        private readonly string[] tags;
        private readonly DateTimeOffset? resolvedOn;
        private readonly Contributor[] contributors;

        public ReleaseNoteItem(string title, string issueNumber, Uri htmlUrl, string[] tags, DateTimeOffset? resolvedOn, Contributor[] contributors)
        {
            this.title = title;
            this.issueNumber = issueNumber;
            this.htmlUrl = htmlUrl;
            this.tags = tags ?? new string[0];
            this.resolvedOn = resolvedOn;
            this.contributors = contributors;
        }

        public string Title
        {
            get { return title; }
        }

        public Uri HtmlUrl
        {
            get { return htmlUrl; }
        }

        public string[] Tags
        {
            get { return tags; }
        }

        public string IssueNumber
        {
            get { return issueNumber; }
        }

        public DateTimeOffset? ResolvedOn
        {
            get { return resolvedOn; }
        }

        public Contributor[] Contributors { get { return contributors; } }

        public string ToString(Categories categories)
        {
            var formattedcategories = FormatCategories(Tags, categories);
            var issueNum = IssueNumber == null ? null : String.Format(" [{0}]", IssueNumber);
            var url = HtmlUrl == null ? null : String.Format("({0})", HtmlUrl);
            var contributors = Contributors == null || Contributors.Length == 0 ?
                string.Empty : " contributed by " + String.Join(", ", Contributors.Select(r => String.Format("{0} ([{1}]({2}))", r.Name, r.Username, r.Url)));

            return string.Format(" - {1}{2}{4}{0}{5}{3}", Title, issueNum, url, formattedcategories,
                Title.TrimStart().StartsWith("-") ? null : " - ",
                contributors).Replace("  ", " ").Replace("- -", "-");
        }

        private string FormatCategories(string[] tags, Categories categories)
        {
            var taggedCategories = categories.AllLabels ? Tags : new[] { Tags.FirstOrDefault(t => categories.AvailableCategories.Any(c => c.Equals(t, StringComparison.InvariantCultureIgnoreCase))) };

            if(taggedCategories == null || (taggedCategories.Length == 1 && string.IsNullOrEmpty(taggedCategories[0])))
            {
                return null;
            }

            for (int i = 0; i < taggedCategories.Length; i++)
            {
                if ("bug".Equals(taggedCategories[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    taggedCategories[i] = "fix";
                }
                taggedCategories[i] = string.Concat(" +", taggedCategories[i].Replace(" ", "-"));
            }

            return string.Join(string.Empty, taggedCategories);

        }
    }
}