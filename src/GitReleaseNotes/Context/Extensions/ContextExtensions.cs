using GitTools;
using GitTools.IssueTrackers;

namespace GitReleaseNotes
{
    public static class ContextExtensions
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        // TODO: Use IValidationContext
        public static bool Validate(this Context context)
        {
            if (!context.Repository.Validate())
            {
                return false;
            }

            if (!context.IssueTracker.Validate())
            {
                return false;
            }

            return true;
        }

        public static string GetContextKey(this Context context)
        {
            var key = string.Join("_", context.Repository.Url, context.Repository.Branch, context.IssueTracker.Server, context.IssueTracker.ProjectId);

            key = key.Replace("/", "_")
                .Replace("\\", "_")
                .Replace(":", "_");

            return key;
        }
    }
}
