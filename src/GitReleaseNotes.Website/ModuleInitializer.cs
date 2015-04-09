using Catel.IoC;
using GitReleaseNotes;
using GitReleaseNotes.FileSystem;
using GitReleaseNotes.IssueTrackers;
using GitReleaseNotes.Website.Services;
using FileSystem = GitReleaseNotes.Website.Services.FileSystem;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        var serviceLocator = ServiceLocator.Default;

        serviceLocator.RegisterType<IReleaseNotesService, ReleaseNotesService>();
        serviceLocator.RegisterType<IFileSystem, FileSystem>();
        serviceLocator.RegisterType<IIssueTrackerFactory, IssueTrackerFactory>();
    }
}