using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace PSOK.Kademlia.Web.Areas
{
    /// <summary>
    /// Extension methods for the <see cref="AreaRegistrationContext" /> class.
    /// </summary>
    public static class AreaRegistrationContextExtensions
    {
        /// <summary>
        /// Maps the specified route template and sets the default route values and constraints.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="routeTemplate"></param>
        /// <returns></returns>
        public static Route MapHttpRoute(this AreaRegistrationContext context, string name, string routeTemplate)
        {
            return context.MapHttpRoute(name, routeTemplate, null, null);
        }

        /// <summary>
        /// Maps the specified route template and sets the default route values and constraints.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="routeTemplate"></param>
        /// <param name="defaults"></param>
        /// <returns></returns>
        public static Route MapHttpRoute(this AreaRegistrationContext context, string name, string routeTemplate,
            object defaults)
        {
            return context.MapHttpRoute(name, routeTemplate, defaults, null);
        }

        /// <summary>
        /// Maps the specified route template and sets the default route values and constraints.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="routeTemplate"></param>
        /// <param name="defaults"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public static Route MapHttpRoute(this AreaRegistrationContext context, string name, string routeTemplate,
            object defaults, object constraints)
        {
            Route route = context.Routes.MapHttpRoute(name, routeTemplate, defaults, constraints);
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }
            route.DataTokens.Add("area", context.AreaName);
            return route;
        }
    }
}