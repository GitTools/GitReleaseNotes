

using System.Threading.Tasks;
using Catel;
using Catel.IoC;

namespace GitReleaseNotes.Website.Services
{
    using System;
    using Catel.Logging;
    using Catel.Caching;
    using Catel.Caching.Policies;

    public class ReleaseNotesService : IReleaseNotesService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly ITypeFactory _typeFactory;

        public ReleaseNotesService(ITypeFactory typeFactory)
        {
            Argument.IsNotNull(() => typeFactory);

            _typeFactory = typeFactory;
        }

        public async Task<SemanticReleaseNotes> GetReleaseNotesAsync(ReleaseNotesGenerationParameters generationParameters)
        {
            try
            {
                Log.Info("Generating release notes for '{0}'", "..."); // TODO log properly

                var releaseNotesGenerator = new ReleaseNotesGenerator(generationParameters);
                var releaseNotes = await releaseNotesGenerator.GenerateReleaseNotesAsync(new SemanticReleaseNotes());
                return releaseNotes;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate release notes for context '{0}'", "...");
                return null;
            }
        }
    }
}
