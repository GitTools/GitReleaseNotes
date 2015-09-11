using System.Threading.Tasks;

namespace GitReleaseNotes.Website.Services
{
    public interface IReleaseNotesService
    {
        Task<SemanticReleaseNotes> GetReleaseNotesAsync(Context context);
    }
}