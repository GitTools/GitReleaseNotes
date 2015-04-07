using System;

namespace GitReleaseNotes.Git
{
    /// <summary>
    /// Responsible for providing information about a branch from it's name.
    /// </summary>
    public class GitBranchNameInfo
    {
        private string branchName;

        public GitBranchNameInfo(string branchName)
        {
            this.branchName = branchName ?? string.Empty;
        }

        public string GetCanonicalBranchName()
        {
            if (IsPullRequest())
            {
                branchName = branchName.Replace("pull-requests", "pull");
                branchName = branchName.Replace("pr", "pull");

                return string.Format("refs/{0}/head", branchName);
            }

            return string.Format("refs/heads/{0}", branchName);
        }

        public bool IsPullRequest()
        {
            return branchName.Contains("pull/") || branchName.Contains("pull-requests/") || branchName.Contains("pr/");
        }

        public bool IsHotfix()
        {
            return branchName.StartsWith("hotfix-") || branchName.StartsWith("hotfix/");
        }

        public string GetHotfixSuffix()
        {
            var result = this.TrimStart(branchName, "hotfix-");
            result = this.TrimStart(result, "hotfix/");
            return result;
        }

        public bool IsRelease()
        {
            return branchName.StartsWith("release-") || branchName.StartsWith("release/");
        }

        public string GetReleaseSuffix()
        {
            var result = this.TrimStart(branchName, "release-");
            result = this.TrimStart(result, "release/");
            return result;
        }

        public string GetUnknownBranchSuffix()
        {
            var unknownBranchSuffix = branchName.Split('-', '/');
            if (unknownBranchSuffix.Length == 1)
            {
                return branchName;
            }

            return unknownBranchSuffix[1];
        }

        public bool IsDevelop()
        {
            return branchName == "develop";
        }

        public bool IsMaster()
        {
            return branchName == "master";
        }

        public bool IsSupport()
        {
            return branchName.ToLower().StartsWith("support-") || branchName.ToLower().StartsWith("support/");
        }

        private string TrimStart(string value, string toTrim)
        {
            if (!value.StartsWith(toTrim, StringComparison.InvariantCultureIgnoreCase))
            {
                return value;
            }

            var startIndex = toTrim.Length;
            return value.Substring(startIndex);
        }
    }
}
