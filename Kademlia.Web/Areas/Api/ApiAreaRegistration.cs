using System.Web.Http;
using System.Web.Mvc;

// ReSharper disable RedundantArgumentName

namespace PSOK.Kademlia.Web.Areas.Api
{
    /// <summary>
    /// Class responsible for registering the Api area.
    /// </summary>
    public class ApiAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// The name of the area.
        /// </summary>
        public override string AreaName
        {
            get { return "Api"; }
        }

        /// <summary>
        /// Registers the Api area.
        /// </summary>
        /// <param name="context"></param>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapHttpRoute(
                name: "Api_default",
                routeTemplate: "Api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );
        }
    }
}

// ReSharper restore RedundantArgumentName