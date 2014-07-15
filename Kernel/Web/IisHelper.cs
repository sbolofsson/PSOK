using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Routing;
using PSOK.Kernel.Collections;
using PSOK.Kernel.Exceptions;
using Microsoft.Web.Administration;
using ServerManagerException = PSOK.Kernel.Exceptions.ServerManagerException;

namespace PSOK.Kernel.Web
{
    /// <summary>
    /// Helper class for working with IIS
    /// </summary>
    public static class IisHelper
    {
        /// <summary>
        /// Assigns a certificate to a website binding
        /// </summary>
        /// <param name="siteInformation">Information identifying the website and binding to assign the certificate to</param>
        public static void AssignCertificateToSite(SiteInformation siteInformation)
        {
            if (siteInformation == null)
                throw new ArgumentNullException("siteInformation");

            Binding binding = GetBinding(siteInformation);

            if (string.IsNullOrEmpty(siteInformation.CertificateHash))
            {
                throw new CertificateException("Certificate hash is required when adding certificate.");
            }

            ConfigurationMethod configurationMethod = binding.Methods.FirstOrDefault(x =>
                string.Equals(x.Name, "AddSslCertificate", StringComparison.InvariantCultureIgnoreCase));

            if (configurationMethod == null)
            {
                throw new CertificateException("Unable to access the AddSslCertificate configuration method.");
            }

            ConfigurationMethodInstance configurationMethodInstance = configurationMethod.CreateInstance();
            configurationMethodInstance.Input.SetAttributeValue("certificateHash", siteInformation.CertificateHash);
            configurationMethodInstance.Input.SetAttributeValue("certificateStoreName", siteInformation.CertificateStore);
            configurationMethodInstance.Execute();
        }

        /// <summary>
        /// Unassigns a certificate to a website binding
        /// </summary>
        /// <param name="siteInformation">Information identifying the website and binding to assign the certificate to</param>
        public static void UnassignCertificateFromSite(SiteInformation siteInformation)
        {
            if (siteInformation == null)
                throw new ArgumentNullException("siteInformation");

            Binding binding = GetBinding(siteInformation);

            ConfigurationMethod configurationMethod = binding.Methods.FirstOrDefault(x =>
                string.Equals(x.Name, "RemoveSslCertificate", StringComparison.InvariantCultureIgnoreCase));

            if (configurationMethod == null)
            {
                throw new CertificateException("Unable to access the RemoveSslCertificate configuration method.");
            }

            ConfigurationMethodInstance configurationMethodInstance = configurationMethod.CreateInstance();
            configurationMethodInstance.Execute();
        }

        /// <summary>
        /// Gets a website binding
        /// </summary>
        /// <param name="siteInformation">Information identifying the website to get the binding from</param>
        /// <returns></returns>
        private static Binding GetBinding(SiteInformation siteInformation)
        {
            if (siteInformation == null)
                throw new ArgumentNullException("siteInformation");

            using (ServerManager serverManager = string.IsNullOrEmpty(siteInformation.Server)
                ? new ServerManager()
                : ServerManager.OpenRemote(siteInformation.Server))
            {
                Site site =
                serverManager.Sites.FirstOrDefault(
                    x => string.Equals(x.Name, siteInformation.SiteName, StringComparison.InvariantCultureIgnoreCase));

                if (site == null)
                    throw new ServerManagerException(string.Format("Could not find site with site name '{0}'.",
                        siteInformation.SiteName));

                Binding binding = site.Bindings.FirstOrDefault(x =>
                    string.Equals(x.Protocol, siteInformation.Protocol, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(x.BindingInformation, siteInformation.Dns, StringComparison.InvariantCultureIgnoreCase));

                if (binding == null)
                    throw new CertificateException(string.Format(
                        "Binding with protocol '{0}' and dns '{1}' was not found.", siteInformation.Protocol,
                        siteInformation.Dns));

                return binding;
            }
        }

        /// <summary>
        /// Retrieve all <see cref="Uri"/>s from the website hosting the current application.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Uri> GetUris()
        {
            return GetUris(new SiteInformation
            {
                SiteName = HostingEnvironment.SiteName
            });
        }

        /// <summary>
        /// Retrieve all <see cref="Uri"/>s from a website matching the given site information.
        /// </summary>
        /// <param name="siteInformation"></param>
        /// <returns></returns>
        public static IEnumerable<Uri> GetUris(SiteInformation siteInformation)
        {
            if (siteInformation == null)
                throw new ArgumentNullException("siteInformation");

            if (string.IsNullOrEmpty(siteInformation.SiteName))
                return new List<Uri>();

            using (ServerManager serverManager = string.IsNullOrEmpty(siteInformation.Server)
                ? new ServerManager()
                : ServerManager.OpenRemote(siteInformation.Server))
            {
                Site site =
                serverManager.Sites.FirstOrDefault(
                    x => string.Equals(x.Name, siteInformation.SiteName, StringComparison.InvariantCultureIgnoreCase));

                if (site == null)
                    throw new ServerManagerException(string.Format("Could not find site with site name '{0}'.",
                        siteInformation.SiteName));

                if (site.Bindings == null || !site.Bindings.Any())
                    return new List<Uri>();

                IEnumerable<string> hostNames = site.Bindings
                    .Select(GetHostName)
                    .Where(x => x != null).ToList();

                if (!hostNames.Any())
                    return new List<Uri>();

                IEnumerable<string> relativePaths = GetAllMvcRoutes()
                    .Union(GetDefaultDocuments(siteInformation));

                return hostNames.Select(x => new Uri(x))
                    .Union(new[] { hostNames, relativePaths }
                    .CartesianProduct()
                    .Select(x => new Uri(string.Join(string.Empty, x))))
                    .Distinct()
                    .ToList();
            }
        }

        /// <summary>
        /// Retrieves all default documents for the current application.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetDefaultDocuments()
        {
            return GetDefaultDocuments(new SiteInformation
            {
                SiteName = HostingEnvironment.SiteName
            });
        }

        /// <summary>
        /// Retrieves all default documents for a server and site matching the specified <see cref="SiteInformation"/>.
        /// </summary>
        /// <param name="siteInformation"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetDefaultDocuments(SiteInformation siteInformation)
        {
            if (siteInformation == null)
                throw new ArgumentNullException("siteInformation");

            if (string.IsNullOrEmpty(siteInformation.SiteName))
                return new List<string>();

            using (ServerManager serverManager = string.IsNullOrEmpty(siteInformation.Server)
                ? new ServerManager()
                : ServerManager.OpenRemote(siteInformation.Server))
            {
                // Retrieve default documents
                Microsoft.Web.Administration.Configuration webConfig = serverManager.GetWebConfiguration(siteInformation.SiteName);
                ConfigurationSection section = webConfig.GetSection("system.webServer/defaultDocument");
                return section.GetCollection("files").Select(x => (string)x["value"])
                    .ToList();
            }
        } 

        /// <summary>
        /// Retrieves all MVC routes in the current application.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAllMvcRoutes()
        {
            // Retrieve mvc routes
            IEnumerable<Route> routes = RouteTable.Routes.OfType<Route>()
                .Where(x => (x.DataTokens == null || !x.DataTokens.ContainsKey("area")) &&
                    x.Defaults != null && !(x.RouteHandler is StopRoutingHandler))
                .ToList();

            List<string> routeUrls = new List<string>();

            foreach (Route route in routes)
            {
                string url = route.Url;
                foreach (KeyValuePair<string, object> routeValue in route.Defaults)
                {
                    string key = string.Format("{{{0}}}", routeValue.Key);
                    string value = routeValue.Value != null ? routeValue.Value.ToString() : string.Empty;
                    url = url.Replace(key, value);
                }

                url = Regex.Replace(url, "[/]{2,}", "/");
                url = url.TrimEnd('/');

                routeUrls.Add(url);
            }

            return routeUrls;
        } 

        /// <summary>
        /// Retrieves the hosts name from a <see cref="Microsoft.Web.Administration.Binding"/>.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static string GetHostName(Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException("binding");

            if (!string.Equals(binding.Protocol, "http", StringComparison.InvariantCultureIgnoreCase) &&
                !string.Equals(binding.Protocol, "https", StringComparison.InvariantCultureIgnoreCase))
                return null;

            string[] bindingInfo = binding.BindingInformation.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (bindingInfo.Length != 3)
                return null;

            string scheme = binding.Protocol;
            string ip = bindingInfo[0];
            string port = bindingInfo[1];
            string hostName = bindingInfo[2];

            // Port number should always be specified but take care anyway
            int portNumber;
            bool portSpecified = int.TryParse(port, out portNumber);

            // IIS uses * to indicate any IP (same as none specified)
            if (string.Equals(ip, "*") || string.Equals(ip, string.Empty))
                ip = null;

            // Prefer hostname, but fall back to ip and eventually just "localhost"
            if (string.IsNullOrEmpty(hostName))
                hostName = ip ?? "localhost";

            return (portSpecified ?
                new UriBuilder(scheme, hostName, portNumber) :
                new UriBuilder(scheme, hostName)).Uri.ToString();
        }

        /// <summary>
        /// Indicates if the current process is hosted by IIS or IIS express.
        /// </summary>
        /// <returns></returns>
        public static bool IsHostedByIis
        {
            get
            {
                return !string.IsNullOrEmpty(HostingEnvironment.SiteName) || HostingEnvironment.IsHosted;
            }
        }
    }
}
