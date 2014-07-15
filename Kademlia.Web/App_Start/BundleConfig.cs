using System.Web.Optimization;

namespace PSOK.Kademlia.Web
{
    /// <summary>
    /// Configuration for script and css bundles.
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// Registers scripts and css for the application.
        /// </summary>
        /// <param name="bundleCollection"></param>
        public static void RegisterBundles(BundleCollection bundleCollection)
        {
            bundleCollection.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundleCollection.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundleCollection.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            bundleCollection.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/site.css"));

            bundleCollection.Add(new StyleBundle("~/Content/kademlia").Include(
                "~/Content/kademlia.css"));

            bundleCollection.Add(new ScriptBundle("~/bundles/kademlia").Include(
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/ajaxmanager.js",
                "~/Scripts/report.js"));

            BundleTable.EnableOptimizations = true;
        }
    }
}