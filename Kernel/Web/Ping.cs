using System;
using System.Net;

namespace PSOK.Kernel.Web
{
    /// <summary>
    /// A http ping to a website.
    /// </summary>
    public class Ping
    {
        private readonly Uri _url;

        /// <summary>
        /// Constructs a new <see cref="Ping"/> targeted at the specified url.
        /// </summary>
        /// <param name="url"></param>
        public Ping(Uri url)
        {
            _url = url;
        }

        /// <summary>
        /// Executes a ping against the specified url.
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(_url);
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                return webResponse.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                { }
            }
            return false;
        }
    }
}
