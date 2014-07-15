using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using PSOK.Kernel.Reflection;

namespace PSOK.Kademlia.Web.Areas
{
    /// <summary>
    /// This class was taken from http://blogs.infosupport.com/asp-net-mvc-4-rc-getting-webapi-and-areas-to-play-nicely/
    /// in order to make Web API controllers work as an area.
    /// </summary>
    public class AreaHttpControllerSelector : DefaultHttpControllerSelector
    {
        private const string AreaRouteVariableName = "area";

        private readonly HttpConfiguration _httpConfiguration;
        private readonly Lazy<ConcurrentDictionary<string, Type>> _apiControllerTypes;

        /// <summary>
        /// Constructs a new <see cref="AreaHttpControllerSelector"/>.
        /// </summary>
        /// <param name="httpConfiguration"></param>
        public AreaHttpControllerSelector(HttpConfiguration httpConfiguration)
            : base(httpConfiguration)
        {
            _httpConfiguration = httpConfiguration;
            _apiControllerTypes = new Lazy<ConcurrentDictionary<string, Type>>(GetControllerTypes);
        }

        /// <summary>
        /// Selects a controller based on the specified <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <returns></returns>
        public override HttpControllerDescriptor SelectController(HttpRequestMessage httpRequestMessage)
        {
            return GetApiController(httpRequestMessage);
        }

        private static string GetAreaName(HttpRequestMessage httpRequestMessage)
        {
            IHttpRouteData httpRouteData = httpRequestMessage.GetRouteData();
            if (httpRouteData.Route.DataTokens == null)
            {
                return null;
            }
            object areaName;
            return httpRouteData.Route.DataTokens.TryGetValue(AreaRouteVariableName, out areaName)
                ? areaName.ToString()
                : null;
        }

        private static ConcurrentDictionary<string, Type> GetControllerTypes()
        {
            IEnumerable<Assembly> assemblies = AssemblyHelper.GetAllAssemblies();

            IDictionary<string, Type> controllerTypes = assemblies
                .SelectMany(x => x.GetTypes()
                    .Where(y => !y.IsAbstract &&
                                y.Name.EndsWith(ControllerSuffix, StringComparison.InvariantCultureIgnoreCase) &&
                                typeof (IHttpController).IsAssignableFrom(y)))
                .ToDictionary(x => x.FullName, x => x);

            return new ConcurrentDictionary<string, Type>(controllerTypes);
        }

        private HttpControllerDescriptor GetApiController(HttpRequestMessage httpRequestMessage)
        {
            string areaName = GetAreaName(httpRequestMessage);
            string controllerName = GetControllerName(httpRequestMessage);
            Type controllerType = GetControllerType(areaName, controllerName);

            return new HttpControllerDescriptor(_httpConfiguration, controllerName, controllerType);
        }

        private Type GetControllerType(string areaName, string controllerName)
        {
            IEnumerable<KeyValuePair<string, Type>> query = _apiControllerTypes.Value.AsEnumerable();
            query = string.IsNullOrEmpty(areaName) ? query.WithoutAreaName() : query.ByAreaName(areaName);
            return query
                .ByControllerName(controllerName)
                .Select(x => x.Value)
                .Single();
        }
    }

    internal static class ControllerTypeExtensions
    {
        public static IEnumerable<KeyValuePair<string, Type>> ByAreaName(
            this IEnumerable<KeyValuePair<string, Type>> query, string areaName)
        {
            string areaNameToFind = string.Format(CultureInfo.InvariantCulture, ".{0}.", areaName);
            return query.Where(x => x.Key.IndexOf(areaNameToFind, StringComparison.InvariantCultureIgnoreCase) != -1);
        }

        public static IEnumerable<KeyValuePair<string, Type>> WithoutAreaName(
            this IEnumerable<KeyValuePair<string, Type>> query)
        {
            return query.Where(x => x.Key.IndexOf(".areas.", StringComparison.InvariantCultureIgnoreCase) == -1);
        }

        public static IEnumerable<KeyValuePair<string, Type>> ByControllerName(
            this IEnumerable<KeyValuePair<string, Type>> query, string controllerName)
        {
            string controllerNameToFind = string.Format(CultureInfo.InvariantCulture, ".{0}{1}", controllerName,
                DefaultHttpControllerSelector.ControllerSuffix);
            return query.Where(x => x.Key.EndsWith(controllerNameToFind, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}