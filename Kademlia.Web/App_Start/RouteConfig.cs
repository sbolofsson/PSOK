using System.Web.Mvc;
using System.Web.Routing;

// ReSharper disable RedundantArgumentName

namespace PSOK.Kademlia.Web
{
    /// <summary>
    /// Configuration for routes.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Registers routes.
        /// </summary>
        /// <param name="routeCollection"></param>
        public static void RegisterRoutes(RouteCollection routeCollection)
        {
            routeCollection.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routeCollection.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}

// ReSharper restore RedundantArgumentName