using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PSOK.Kernel.Web
{
    /// <summary>
    /// Information about a binding of an IIS website
    /// </summary>
    public class SiteInformation
    {
        /// <summary>
        /// The server name which the website resides on.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// The site name of the website.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// The protocol of the binding.
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// The host name of the binding.
        /// </summary>
        public string Dns { get; set; }

        /// <summary>
        /// The certificate hash used for the binding.
        /// </summary>
        public string CertificateHash { get; set; }

        /// <summary>
        /// The certificate store of the certificate used for the binding.
        /// </summary>
        public string CertificateStore { get; set; }

        /// <summary>
        /// Returns <see cref="SiteInformation"/> properties as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetType().GetProperties().Select(x => new {x.Name, Value = x.GetValue(this)})
                .Aggregate(string.Empty,
                    (acc, property) => string.Format("{0}{1}: {2}\n", acc, property.Name, property.Value));
        }

        /// <summary>
        /// Gets all the site information properties as a string of arguments.
        /// </summary>
        /// <returns></returns>
        public static string GetPropertiesAsArgumentList()
        {
            return typeof (SiteInformation).GetProperties().Select(x => x.Name)
                .Aggregate((acc, cur) => string.Format("{0} -{1}=[{1}]", acc, cur)).Trim();
        }

        /// <summary>
        /// Creates a new <see cref="SiteInformation" /> instance based on the specified key/value parameters.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static SiteInformation CreateFromParams(IDictionary<string, string> param)
        {
            SiteInformation siteInformation = new SiteInformation();
            foreach (PropertyInfo propertyInfo in typeof (SiteInformation).GetProperties())
            {
                string key = propertyInfo.Name.ToLower();
                if (param.ContainsKey(key))
                    propertyInfo.SetValue(siteInformation, param[key]);
            }
            return siteInformation;
        }
    }
}