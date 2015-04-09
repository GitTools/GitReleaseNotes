namespace GitReleaseNotes
{
    public static class IIssueTrackerContextExtensions
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        // TODO: Use IValidationContext
        public static bool Validate(this IIssueTrackerContext context)
        {
            if (string.IsNullOrWhiteSpace(context.ProjectId))
            {
                Log.WriteLine("IssueTracker.ProjectId is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(context.Url))
            {
                Log.WriteLine("IssueTracker.Url is required");
                return false;
            }

            return true;
        }
    }
}