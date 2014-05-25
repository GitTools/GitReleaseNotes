using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GitReleaseNotes
{
    public class SemanticReleaseNotes
    {
        //readonly Regex _issueRegex = new Regex(" - (?<Issue>.*?)(?<IssueLink> \\[(?<IssueId>.*?)\\]\\((?<IssueUrl>.*?)\\))*( *\\+(?<Tag>[^ \\+]*))*", RegexOptions.Compiled);
        static readonly Regex ReleaseRegex = new Regex("# (?<Title>.*?)( \\((?<Date>.*?)\\))?$", RegexOptions.Compiled);
        readonly string[] _categories;
        readonly SemanticRelease[] _releases;

        public SemanticReleaseNotes()
        {
            _categories = new string[0];
            _releases = new SemanticRelease[0];
        }

        public SemanticReleaseNotes(IEnumerable<SemanticRelease> releaseNoteItems, string[] categories)
        {
            _categories = categories;
            _releases = releaseNoteItems.ToArray();
        }

        public SemanticRelease[] Releases
        {
            get { return _releases; }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (var index = 0; index < Releases.Length; index++)
            {
                if (index > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }

                var release = Releases[index];
                if (Releases.Length > 1)
                {
                    var hasBeenReleased = String.IsNullOrEmpty(release.ReleaseName);
                    if (hasBeenReleased)
                        builder.AppendLine("# vNext");
                    else if (release.When != null)
                        builder.AppendLine(String.Format("# {0} ({1:dd MMMM yyyy})", release.ReleaseName,
                            release.When.Value.Date));
                    else
                        builder.AppendLine(String.Format("# {0}", release.ReleaseName));

                    builder.AppendLine();
                }

                IEnumerable<ReleaseNoteItem> releaseNoteItems = release.ReleaseNoteItems;
                foreach (var releaseNoteItem in releaseNoteItems)
                {
                    var taggedCategory = releaseNoteItem.Tags
                        .FirstOrDefault(t => _categories.Any(c => c.Equals(t, StringComparison.InvariantCultureIgnoreCase)));
                    var title = releaseNoteItem.Title;
                    var issueNumber = releaseNoteItem.IssueNumber;
                    var htmlUrl = releaseNoteItem.HtmlUrl;
                    if ("bug".Equals(taggedCategory, StringComparison.InvariantCultureIgnoreCase))
                        taggedCategory = "fix";
                    var category = taggedCategory == null
                        ? null
                        : String.Format(" +{0}", taggedCategory.Replace(" ", "-"));
                    var issueNum = issueNumber == null ? null : String.Format(" [{0}]", issueNumber);
                    var url = htmlUrl == null ? null : String.Format("({0})", htmlUrl);
                    var contributors = releaseNoteItem.Contributors == null || releaseNoteItem.Contributors.Length == 0 ?
                        string.Empty : " contributed by " + String.Join(", ", releaseNoteItem.Contributors.Select(r => String.Format("{0} ([{1}]({2}))", r.Name, r.Username, r.Url)));
                    var item = String.Format(" - {1}{2}{4}{0}{5}{3}", title, issueNum, url, category,
                        title.TrimStart().StartsWith("-") ? null : " - ",
                        contributors).Replace("  ", " ").Replace("- -", "-");
                    builder.AppendLine(item);
                }

                builder.AppendLine();
                if (release.DiffInfo.Url == null)
                    builder.AppendLine(String.Format("Commits: {0}...{1}", release.DiffInfo.BeginningSha, release.DiffInfo.EndSha));
                else
                {
                    builder.AppendLine(String.Format("Commits: [{0}...{1}]({2})",
                        release.DiffInfo.BeginningSha, release.DiffInfo.EndSha, release.DiffInfo.Url));
                }
            }

            return builder.ToString();
        }

        public static SemanticReleaseNotes Parse(string releaseNotes)
        {
            var releases = new List<SemanticRelease>();
            var lines = releaseNotes.Replace("\r", string.Empty).Split('\n');

            var currentRelease = new SemanticRelease();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.TrimStart().StartsWith("# "))
                {
                    var match = ReleaseRegex.Match(line);
                    if (line != lines.First())
                        releases.Add(currentRelease);
                    currentRelease = new SemanticRelease
                    {
                        ReleaseName = match.Groups["Title"].Value
                    };
                    if (currentRelease.ReleaseName == "vNext")
                        currentRelease.ReleaseName = null;

                    if (match.Groups["Date"].Success)
                        currentRelease.When = DateTime.ParseExact(match.Groups["Date"].Value, "dd MMMM yyyy", CultureInfo.CurrentCulture);
                }
                //TODO Need to support multiple Url's in the release notes
                //else if (line.StartsWith(" - "))
                //{
                //    var match = _issueRegex.Match(line);
                //    var issue = match.Groups["Issue"].Value;
                //    var issueNumber = match.Groups["IssueId"].Value;
                //    var htmlUrl = match.Groups["IssueUrl"].Success ? new Uri(match.Groups["IssueUrl"].Value) : null;
                //    var tags = match.Groups["Tag"].Captures.OfType<Capture>().Select(c => c.Value).ToArray();
                //    var releaseNoteItem = new ReleaseNoteItem(issue, issueNumber, htmlUrl, tags, currentRelease.When);
                //    currentRelease.ReleaseNoteItems.Add(releaseNoteItem);
                //}
                else if (line.StartsWith("Commits: "))
                {
                    var commits = line.Replace("Commits: ", string.Empty).Split(new[] { "..." }, StringSplitOptions.None);
                    currentRelease.DiffInfo.BeginningSha = commits[0];
                    currentRelease.DiffInfo.EndSha = commits[1];
                }
                else
                {
                    // This picks up comments and such
                    var title = line.StartsWith(" - ") ? line.Substring(3) : line;
                    var releaseNoteItem = new ReleaseNoteItem(title, null, null, null, currentRelease.When, new Contributor[0]);
                    currentRelease.ReleaseNoteItems.Add(releaseNoteItem);
                }
            }

            releases.Add(currentRelease);

            return new SemanticReleaseNotes(releases, new string[0]);
        }

        public SemanticReleaseNotes Merge(SemanticReleaseNotes previousReleaseNotes)
        {
            var mergedReleases =
                previousReleaseNotes.Releases
                .Where(r => Releases.All(r2 => r.ReleaseName != r2.ReleaseName))
                .Select(CreateMergedSemanticRelease)
                .Union(Releases.Select(CreateMergedSemanticRelease))
                .ToArray();

            foreach (var semanticRelease in mergedReleases)
            {
                var releaseFromThis = Releases.SingleOrDefault(r => r.ReleaseName == semanticRelease.ReleaseName);
                var releaseFromPrevious = previousReleaseNotes.Releases.SingleOrDefault(r => r.ReleaseName == semanticRelease.ReleaseName);

                if (releaseFromThis != null)
                {
                    semanticRelease.ReleaseNoteItems.AddRange(releaseFromThis.ReleaseNoteItems);
                }
                if (releaseFromPrevious != null)
                {
                    semanticRelease.ReleaseNoteItems.AddRange(releaseFromPrevious.ReleaseNoteItems);
                }
            }

            return new SemanticReleaseNotes(mergedReleases, _categories.Union(previousReleaseNotes._categories).Distinct().ToArray());
        }

        private static SemanticRelease CreateMergedSemanticRelease(SemanticRelease r)
        {
            return new SemanticRelease(r.ReleaseName, r.When, new List<ReleaseNoteItem>(), r.DiffInfo);
        }
    }
}