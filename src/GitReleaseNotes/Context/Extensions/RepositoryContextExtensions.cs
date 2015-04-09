namespace GitReleaseNotes
{
    public static class RepositoryContextExtensions
    {
        private static readonly ILog Log = GitReleaseNotesEnvironment.Log;

        // TODO: Use IValidationContext
        public static bool Validate(this RepositoryContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Url))
            {
                Log.WriteLine("Repository.Url is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(context.Branch))
            {
                Log.WriteLine("Repository.Branch is required");
                return false;
            }

            return true;
        }
    }
}