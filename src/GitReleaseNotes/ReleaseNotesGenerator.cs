using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitReleaseNotes
{
    public class ReleaseNotesGenerator
    {
        private readonly string[] _categories = { "bug", "enhancement", "feature" };

        public string GenerateReleaseNotes(
            GitReleaseNotesArguments arguments,
            SemanticReleaseNotes releaseNotes,
            SemanticReleaseNotes previousReleaseNotes)
        {
            var builder = new StringBuilder();
            var categories = arguments.Categories == null ? _categories : _categories.Concat(arguments.Categories.Split(',')).ToArray();
            for (var index = 0; index < releaseNotes.Releases.Length; index++)
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

                var previousMatchingRelease = previousReleaseNotes.Releases
                    .SingleOrDefault(r => r.ReleaseName == release.ReleaseName);

                IEnumerable<ReleaseNoteItem> releaseNoteItems;
                if (previousMatchingRelease == null)
                    releaseNoteItems = release.ReleaseNoteItems;
                else
                {
                    releaseNoteItems = previousMatchingRelease.ReleaseNoteItems
                        .Concat(release.ReleaseNoteItems.Where(r => r.ResolvedOn > previousMatchingRelease.When));
                }
                foreach (var releaseNoteItem in releaseNoteItems)
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
                    var issueNum = issueNumber == null ? null : string.Format(" [{0}]", issueNumber);
                    var url = htmlUrl == null ? null : string.Format("({0})", htmlUrl);
                    var contributors = releaseNoteItem.Contributors == null || releaseNoteItem.Contributors.Length == 0 ?
                        string.Empty : " contributed by " + string.Join(", ", releaseNoteItem.Contributors.Select(r => string.Format("{0} ([{1}]({2}))", r.Name, r.Username, r.Url)));
                    var item = string.Format("{4}{0}{1}{2}{5}{3}", title, issueNum, url, category,
                        title.TrimStart().StartsWith("-") ? null : " - ",
                        contributors);
                    builder.AppendLine(item);
                }

                builder.AppendLine();
                if (release.DiffInfo.Url == null)
                    builder.AppendLine(string.Format("Commits: {0}...{1}", release.DiffInfo.BeginningSha, release.DiffInfo.EndSha));
                else
                {
                    builder.AppendLine(string.Format("Commits: [{0}...{1}]({2})",
                        release.DiffInfo.BeginningSha, release.DiffInfo.EndSha, release.DiffInfo.Url));
                }
            }

            return builder.ToString();
        }
    }
}