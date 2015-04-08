

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

        public ReleaseNotesService()
        {
            
        }

        public SemanticReleaseNotes GetReleaseNotes(Context context)
        {
            var key = context.GetContextKey();

            return _releaseNotesCacheStorage.GetFromCacheOrFetch(key, () =>
            {
                Log.Info("Generating release notes for context '{0}'", key);

                return ReleaseNotesGenerator.GenerateReleaseNotes(context);
            });
        }
    }
}
