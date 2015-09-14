

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

        private readonly ICacheStorage<string, SemanticReleaseNotes> _releaseNotesCacheStorage =
            new CacheStorage<string, SemanticReleaseNotes>(() => ExpirationPolicy.Duration(TimeSpan.FromHours(1)));

        private readonly ITypeFactory _typeFactory;

        public ReleaseNotesService(ITypeFactory typeFactory)
        {
            Argument.IsNotNull(() => typeFactory);

            _typeFactory = typeFactory;
        }

        public async Task<SemanticReleaseNotes> GetReleaseNotesAsync(ReleaseNotesGenerationParameters generationParameters)
        {
            //var cachedReleaseNotes = _releaseNotesCacheStorage.GetFromCacheOrFetchAsync(key, async () =>
            //{
            try
            {
                Log.Info("Generating release notes for '{0}'", "..."); // TODO log properly

                var releaseNotesGenerator = _typeFactory.CreateInstanceWithParametersAndAutoCompletion<ReleaseNotesGenerator>(generationParameters);
                var releaseNotes = await releaseNotesGenerator.GenerateReleaseNotesAsync();
                return releaseNotes;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate release notes for context '{0}'", "...");
                return null;
            }
            //});

            //return cachedReleaseNotes;
        }

        //public static string GetContextKey(Context context)
        //{
        //    var key = string.Join("_", context.Repository.Url, context.Repository.Branch, context.IssueTracker.Server, context.IssueTracker.ProjectId);

        //    key = key.Replace("/", "_")
        //        .Replace("\\", "_")
        //        .Replace(":", "_");

        //    return key;
        //}
    }
}
