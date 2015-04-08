namespace GitReleaseNotes.Website.Services
{
    public class FileSystem : GitReleaseNotes.FileSystem.FileSystem
    {
        public override string GetRepositoryWorkingDirectory(Context context)
        {
            // TODO: fix azure stuff
            return base.GetRepositoryWorkingDirectory(context);
        }
    }
}
