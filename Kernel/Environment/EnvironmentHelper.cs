using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web.Hosting;

namespace PSOK.Kernel.Environment
{
    /// <summary>
    /// Helper class for interacting with the OS.
    /// </summary>
    public static class EnvironmentHelper
    {
        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <returns></returns>
        public static string GetEnvironmentPath()
        {
            return HostingEnvironment.MapPath("~/") ?? AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Gets the locally available IP address of the current machine.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIp()
        {
            IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ipAddress == null ? null : ipAddress.ToString();
        }

        /// <summary>
        /// Gets the publicly available IP address of the current machine.
        /// </summary>
        /// <returns></returns>
        public static string GetPublicIp()
        {
            string html = null;
            try
            {
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream == null)
                        return null;

                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        html = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception) { { } }

            if (html == null)
                return null;

            int startIndex = html.IndexOf("Address: ", StringComparison.InvariantCultureIgnoreCase);

            if (startIndex < 0)
                return null;

            startIndex += 9;

            int lastIndex = html.LastIndexOf("</body>", StringComparison.InvariantCultureIgnoreCase);

            if (lastIndex < 0 || lastIndex <= startIndex)
                return null;

            return html.Substring(startIndex, lastIndex - startIndex);
        }

        /// <summary>
        /// Retrieves the FQDN of the local machine
        /// </summary>
        /// <returns></returns>
        public static string GetFqdn()
        {
            string domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            if (!hostName.Contains(domainName))
            {
                hostName = hostName + "." + domainName;
            }

            return hostName;
        }
    }
}