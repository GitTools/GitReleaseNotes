namespace GitReleaseNotes.Website
{
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
#if DEBUG
            LogManager.AddDebugListener(true);
#endif

            DependencyInjectionConfig.RegisterServiceLocatorAsDependencyResolver();
            GlobalConfiguration.Configuration.DependencyResolver = new CatelWebApiDependencyResolver();
        }
        #endregion
    }
}