

namespace GitReleaseNotes.Website
{
    using GitReleaseNotes.Website.Logging;
    using System.Web;
    using System.Web.Http;
    using Catel.Logging;
    using Catel.Mvc;
    using IoC;

    public class Global : HttpApplication
    {
        #region Methods
        protected void Application_Start()
        {
            GitReleaseNotesEnvironment.Log = new GitReleaseNotesLogger();

#if DEBUG
            LogManager.AddDebugListener(true);
#endif

            DependencyInjectionConfig.RegisterServiceLocatorAsDependencyResolver();
            GlobalConfiguration.Configuration.DependencyResolver = new CatelWebApiDependencyResolver();
        }
        #endregion
    }
}