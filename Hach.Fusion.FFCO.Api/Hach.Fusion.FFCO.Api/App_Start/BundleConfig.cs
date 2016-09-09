using System.Web.Optimization;

namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Configuration for bundling multiple files in order to improve service performance.
    /// </summary>
    /// <remarks>
    /// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
    /// </remarks>
    public class BundleConfig
    {
        /// <summary>
        /// Register bundles to improve request load times.
        /// </summary>
        /// <param name="bundles">Collection of bundles that need to be registered.</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
