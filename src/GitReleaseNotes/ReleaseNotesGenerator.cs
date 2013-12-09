using System;
using System.Linq;
using System.Text;

namespace GitReleaseNotes
{
    public class ReleaseNotesGenerator
    {
        private readonly string[] _categories = { "bug", "enhancement", "feature" };
 
        public string GenerateReleaseNotes(GitReleaseNotesArguments arguments, SemanticReleaseNotes releaseNotes)
        {
            var builder = new StringBuilder();
            var categories = arguments.Categories == null ? _categories : _categories.Concat(arguments.Categories.Split(',')).ToArray();
            for (int index = 0; index < releaseNotes.Releases.Length; index++)
            {
                if (index > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }

                var release = releaseNotes.Releases[index];
                if (releaseNotes.Releases.Length > 1)
                {
                    var hasBeenReleased = string.IsNullOrEmpty(release.ReleaseName);
                    if (hasBeenReleased)
                        builder.AppendLine("# vNext");
                    else if (release.When != null)
                        builder.AppendLine(string.Format("# {0} ({1:dd MMMM yyyy})", release.ReleaseName,
                            release.When.Value.Date));
                    else
                        builder.AppendLine(string.Format("# {0}", release.ReleaseName));

                    builder.AppendLine();
                }

                foreach (var releaseNoteItem in release.ReleaseNoteItems)
                {
                    var taggedCategory = releaseNoteItem.Tags
                        .FirstOrDefault(
                            t => categories.Any(c => c.Equals(t, StringComparison.InvariantCultureIgnoreCase)));
                    var title = releaseNoteItem.Title;
                    var issueNumber = releaseNoteItem.IssueNumber;
                    var htmlUrl = releaseNoteItem.HtmlUrl;
                    if ("bug".Equals(taggedCategory, StringComparison.InvariantCultureIgnoreCase))
                        taggedCategory = "fix";
                    var category = taggedCategory == null
                        ? null
                        : string.Format(" +{0}", taggedCategory.Replace(" ", "-"));
                    var item = string.Format(" - {0} [{1}]({2}){3}", title, issueNumber, htmlUrl, category);
                    builder.AppendLine(item);
                }
            }

            return builder.ToString();
        }
    }
}