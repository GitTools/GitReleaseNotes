namespace GitReleaseNotes.Website
{
    using System.Web.Mvc;
    using Filters;
    using Owin;

    public partial class Startup
    {
        private void ConfigureGlobalFilters(IAppBuilder app)
        {
            var filters = GlobalFilters.Filters;

            filters.Add(new HandleErrorAttribute());
        }
    }
}