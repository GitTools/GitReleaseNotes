namespace GitReleaseNotes.Website
{
    using System.Web.Http;
    using Filters.Api;
    using Formatting;
    using Owin;

    public partial class Startup
    {
        private void ConfigureWebApi(IAppBuilder app)
        {
            GlobalConfiguration.Configure(config =>
            {
                // Web API configuration and services
                // Configure Web API to use only bearer token authentication.
                //config.SuppressDefaultHostAuthentication();
                //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

                // Web API routes
                config.MapHttpAttributeRoutes();

                // Controller Only
                config.Routes.MapHttpRoute(
                    name: "ControllerOnly",
                    routeTemplate: "api/{controller}"
                );

                // Controllers with Actions
                config.Routes.MapHttpRoute(
                    name: "ControllerAndAction",
                    routeTemplate: "api/{controller}/{action}"
                );

                // Controller with ID
                config.Routes.MapHttpRoute(
                    name: "ControllerAndId",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: null
                    //constraints: new { id = @"^\d+$" } // Only integers 
                );

                config.Formatters.Add(new PlainTextFormatter());

                //app.Map("/api", inner => inner.UseWebApi(configuration));
            });
        }
    }
}