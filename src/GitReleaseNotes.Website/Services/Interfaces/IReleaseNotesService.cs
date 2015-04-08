namespace GitReleaseNotes.Website.Services
{
    public interface IReleaseNotesService
    {
        SemanticReleaseNotes GetReleaseNotes(Context context);
    }
}