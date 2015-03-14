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
        static readonly Regex LinkRegex = new Regex(@"\[(?<Text>.*?)\]\((?<Link>.*?)\)$", RegexOptions.Compiled);
        readonly Categories categories;
        readonly SemanticRelease[] releases;

        public SemanticReleaseNotes()
        {
            categories = new Categories();
            releases = new SemanticRelease[0];
        }

        public SemanticReleaseNotes(IEnumerable<SemanticRelease> releaseNoteItems, Categories categories)
        {
            this.categories = categories;
            releases = releaseNoteItems.ToArray();
        }

        public SemanticRelease[] Releases
        {
            get { return releases; }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var index = 0;
            foreach (var release in Releases)
            {
                if (release.ReleaseNoteLines.Count == 0)
                    continue;

                if (index++ > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }

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

                IEnumerable<IReleaseNoteLine> releaseNoteItems = release.ReleaseNoteLines;
                foreach (var releaseNoteItem in releaseNoteItems)
                {
                    builder.AppendLine(releaseNoteItem.ToString(categories));
                }

                builder.AppendLine();
                if (string.IsNullOrEmpty(release.DiffInfo.DiffUrlFormat))
                    builder.AppendLine(String.Format("Commits: {0}...{1}", release.DiffInfo.BeginningSha, release.DiffInfo.EndSha));
                else
                {
                    builder.AppendLine(String.Format("Commits: [{0}...{1}]({2})",
                        release.DiffInfo.BeginningSha, release.DiffInfo.EndSha,
                        string.Format(release.DiffInfo.DiffUrlFormat, release.DiffInfo.BeginningSha, release.DiffInfo.EndSha)));
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
                    {
                        DateTime parsed;
                        var toParse = match.Groups["Date"].Value;
                        if (DateTime.TryParse(toParse, out parsed))
                        {
                            currentRelease.When = parsed;
                        }
                        if (DateTime.TryParseExact(toParse, "dd MMMM yyyy", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out parsed))
                        {
                            currentRelease.When = parsed;
                        }
                        else if (DateTime.TryParseExact(toParse, "MMMM dd, yyyy", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out parsed))
                        {
                            currentRelease.When = parsed;
                        }
                        else
                        {
                            // We failed to parse the date, just append to the end
                            currentRelease.ReleaseName += " (" + toParse + ")";
                        }
                    }
                }
                else if (line.StartsWith("Commits: "))
                {
                    var commitText = line.Replace("Commits: ", string.Empty);
                    var linkMatch = LinkRegex.Match(commitText);
                    if (linkMatch.Success)
                    {
                        commitText = linkMatch.Groups["Text"].Value;
                        currentRelease.DiffInfo.DiffUrlFormat = linkMatch.Groups["Link"].Value;
                    }
                    var commits = commitText.Split(new[] { "..." }, StringSplitOptions.None);
                    currentRelease.DiffInfo.BeginningSha = commits[0];
                    currentRelease.DiffInfo.EndSha = commits[1];
                }
                else if (line.StartsWith(" - "))
                {
                    // Improve this parsing to extract issue numbers etc
                    var title = line.StartsWith(" - ") ? line.Substring(3) : line;
                    var releaseNoteItem = new ReleaseNoteItem(title, null, null, null, currentRelease.When, new Contributor[0]);
                    currentRelease.ReleaseNoteLines.Add(releaseNoteItem);
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    currentRelease.ReleaseNoteLines.Add(new BlankLine());
                }
                else
                {
                    // This picks up comments and such
                    var title = line.StartsWith(" - ") ? line.Substring(3) : line;
                    var releaseNoteItem = new ReleaseNoteLine(title);
                    currentRelease.ReleaseNoteLines.Add(releaseNoteItem);
                }
            }

            releases.Add(currentRelease);

            // Remove additional blank lines
            foreach (var semanticRelease in releases)
            {
                for (int i = 0; i < semanticRelease.ReleaseNoteLines.Count; i++)
                {
                    if (semanticRelease.ReleaseNoteLines[i] is BlankLine)
                        semanticRelease.ReleaseNoteLines.RemoveAt(i--);
                    else
                        break;
                }
                for (int i = semanticRelease.ReleaseNoteLines.Count - 1; i >= 0; i--)
                {
                    if (semanticRelease.ReleaseNoteLines[i] is BlankLine)
                        semanticRelease.ReleaseNoteLines.RemoveAt(i);
                    else
                        break;
                }
            }

            return new SemanticReleaseNotes(releases, new Categories());
        }

        public SemanticReleaseNotes Merge(SemanticReleaseNotes previousReleaseNotes)
        {
            var semanticReleases = previousReleaseNotes.Releases
                .Where(r => Releases.All(r2 => r.ReleaseName != r2.ReleaseName))
                .Select(CreateMergedSemanticRelease);
            var enumerable = Releases.Select(CreateMergedSemanticRelease);
            var mergedReleases =
                enumerable
                .Union(semanticReleases)
                .ToArray();

            foreach (var semanticRelease in mergedReleases)
            {
                var releaseFromThis = Releases.SingleOrDefault(r => r.ReleaseName == semanticRelease.ReleaseName);
                var releaseFromPrevious = previousReleaseNotes.Releases.SingleOrDefault(r => r.ReleaseName == semanticRelease.ReleaseName);

                if (releaseFromThis != null)
                {
                    semanticRelease.ReleaseNoteLines.AddRange(releaseFromThis.ReleaseNoteLines);
                }
                if (releaseFromPrevious != null)
                {
                    semanticRelease.ReleaseNoteLines.AddRange(releaseFromPrevious.ReleaseNoteLines);
                }
            }
            
            return new SemanticReleaseNotes(mergedReleases, new Categories(categories.AvailableCategories.Union(previousReleaseNotes.categories.AvailableCategories).Distinct().ToArray(), categories.AllLabels));
        }

        private static SemanticRelease CreateMergedSemanticRelease(SemanticRelease r)
        {
            return new SemanticRelease(r.ReleaseName, r.When, new List<IReleaseNoteLine>(), r.DiffInfo);
        }
    }
}