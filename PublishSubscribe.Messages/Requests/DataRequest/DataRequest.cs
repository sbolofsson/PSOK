using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

namespace PSOK.PublishSubscribe.Messages.Requests.DataRequest
{
    /// <summary>
    /// Base class for data requests.
    /// </summary>
    [Serializable, DataContract]
    public abstract class DataRequest : Request
    {
        /// <summary>
        /// Executes the <see cref="DataRequest" /> using the specified <see cref="DataRequestParameters" />.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public abstract Message Execute(DataRequestParameters parameters);

        /// <summary>
        /// Resolves a <see cref="DataServiceContext" /> based on the specified <see cref="DataRequestParameters" />.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected DataServiceContext Resolve(DataRequestParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (string.IsNullOrEmpty(parameters.BaseUrl))
                throw new ArgumentException("BaseUrl has not been set.");

            if (string.IsNullOrEmpty(parameters.EntitySet))
                throw new ArgumentException("EntitySet has not been set.");

            DataServiceContext dataServiceContext = new DataServiceContext(new Uri(parameters.Url),
                DataServiceProtocolVersion.V3)
            {
                Credentials = CredentialCache.DefaultNetworkCredentials,
                IgnoreResourceNotFoundException = true,
                IgnoreMissingProperties = true,
                ResolveEntitySet = x => new Uri(parameters.Url),
                //ResolveName = x => entitySet,
                SaveChangesDefaultOptions = SaveChangesOptions.ContinueOnError
            };

            IDictionary<string, string> headers = parameters.Headers;

            if (headers != null && headers.Any())
            {
                dataServiceContext.SendingRequest2 += (sender, args) =>
                {
                    foreach (string key in headers.Keys)
                    {
                        args.RequestMessage.SetHeader(key, headers[key]);
                    }
                };
            }

            return dataServiceContext;
        }
    }
}