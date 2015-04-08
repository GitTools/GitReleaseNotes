[assembly: Microsoft.Owin.OwinStartup(typeof (GitReleaseNotes.Website.Startup))]

namespace GitReleaseNotes.Website
{
    using System.Web.Mvc;
    using Owin;

    public partial class Startup
    {
        #region Methods
        public void Configuration(IAppBuilder app)
        {
            // Note: order is very important

            AreaRegistration.RegisterAllAreas();
            ConfigureGlobalFilters(app);
            ConfigureJson(app);
            ConfigureWebApi(app);
            ConfigureRoutes(app);
            ConfigureBundles(app);
        }
        #endregion
    }
}