namespace GitReleaseNotes.Website
{
    using System.Web.Mvc;
    using System.Web.Routing;
    using Owin;

    public partial class Startup
    {
        private void ConfigureRoutes(IAppBuilder app)
        {
            var routes = RouteTable.Routes;

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}