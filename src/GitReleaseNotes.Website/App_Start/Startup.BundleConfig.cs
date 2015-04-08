namespace GitReleaseNotes.Website
{
    using System.Web.Optimization;
    using Owin;

    public partial class Startup
    {
        private void ConfigureBundles(IAppBuilder app)
        {
            var bundles = BundleTable.Bundles;

            BundleScripts(bundles);
            BundleStyles(bundles);

#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif
        }

        private void BundleScripts(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/content/scripts/jquerybundle")
                .Include("~/content/scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/content/scripts/jqueryvalidationbundle")
                            .Include("~/content/scripts/jquery.validate.js"));

            bundles.Add(new ScriptBundle("~/content/scripts/modernizrbundle")
                .Include("~/content/scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/content/scripts/bootstrapbundle")
                .Include("~/content/scripts/bootstrap.js",
                "~/content/scripts/bootstrap-datepicker.js",
                "~/content/scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/content/scripts/scriptsbundle"));

            bundles.Add(new ScriptBundle("~/content/angular/scriptsbundle")
                .Include("~/content/angular/app.js")
                //.IncludeDirectory("~/content/angular/directives", "*.js", true)
                //.IncludeDirectory("~/content/angular/services", "*.js", true)
                //.IncludeDirectory("~/content/angular/mixins", "*.js", true)
                //.IncludeDirectory("~/content/angular/components", "*.js", true)
                );
        }

        private void BundleStyles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/content/styles/bundle")
                .Include("~/content/styles/bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/content/styles/bootstrap-datepicker3.css", new CssRewriteUrlTransform())
                .Include("~/content/styles/font-awesome.css", new CssRewriteUrlTransform())
                .Include("~/content/styles/site.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/content/angular/styles/bundle")
                //.IncludeDirectory("~/content/angular/directives", "*.css", true)
                );
        }
    }
}