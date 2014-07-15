using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using PSOK.Kademlia.Web.Areas;

namespace PSOK.Kademlia.Web
{
    /// <summary>
    /// The application.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Application start event.
        /// </summary>
        protected void Application_Start()
        {
            MvcHandler.DisableMvcResponseHeader = true;
            GlobalConfiguration.Configuration.Services.Replace(typeof (IHttpControllerSelector),
                new AreaHttpControllerSelector(GlobalConfiguration.Configuration));
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Application pre request headers event.
        /// </summary>
        protected void Application_PreSendRequestHeaders()
        {
            // Remove headers
            Response.Headers.Remove("Server");

            // Add headers
            Response.Headers.Add("X-Content-Type-Options", "nosniff");
            Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            Response.Headers.Add("X-Frame-Options", "sameorigin");
            Response.Headers.Add("X-UA-Compatible", "IE=Edge,chrome=1");
            Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubdomains");

            const string contentSecurity =
                "default-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; frame-src 'self'; options inline-script eval-script; img-src *;";

            Response.Headers.Add("Content-Security-Policy", contentSecurity);
            Response.Headers.Add("X-Content-Security-Policy", contentSecurity);
            Response.Headers.Add("X-Webkit-CSP", contentSecurity);
        }
    }
}