using System.Collections.Generic;

namespace PSOK.PublishSubscribe.Messages.Requests.DataRequest
{
    /// <summary>
    /// Parameters for a <see cref="DataRequest" />.
    /// </summary>
    public class DataRequestParameters
    {
        /// <summary>
        /// The base URL of the data service.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// The entity set name of the data.
        /// </summary>
        public string EntitySet { get; set; }

        /// <summary>
        /// The URL to execute the <see cref="DataRequest" /> against.
        /// </summary>
        public string Url
        {
            get { return string.Format("{0}/{1}", BaseUrl, EntitySet); }
        }

        /// <summary>
        /// Headers which should be added to the <see cref="DataRequest" />.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }
    }
}