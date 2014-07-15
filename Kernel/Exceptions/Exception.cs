using System;
using System.Data.Services.Client;
using System.Net;
using System.ServiceModel;

namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// The base class of custom Exceptions.
    /// </summary>
    public class Exception : System.Exception
    {
        private readonly string _message;
        private readonly System.Exception _innerException;

        /// <summary>
        /// Constructs a new <see cref="Exception"/>.
        /// </summary>
        public Exception()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Exception"/> using the specified message.
        /// </summary>
        /// <param name="message"></param>
        public Exception(string message) : base(message)
        {
            _message = message;
        }

        /// <summary>
        /// Constructs a new <see cref="Exception"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public Exception(string message, System.Exception innerException)
            : base(message, innerException)
        {
            _message = message;
            _innerException = innerException;
        }

        /// <summary>
        /// The message of the <see cref="Exception"/>.
        /// </summary>
        public override string Message
        {
            get { return _message; }
        }

        /// <summary>
        /// Returns the <see cref="Message"/> and possibly also the message of the inner exception.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string message = _message;
            if (_innerException != null)
            {
                message = string.Format("{0} Inner exception: {1}.", message, _innerException.Message);
            }
            return message;
        }

        /// <summary>
        /// Indicates whether the specified <see cref="Exception"/> is
        /// related to an endpoint not being available.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static bool IsEndpointDown(System.Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            EndpointNotFoundException endpointNotFoundException = exception as EndpointNotFoundException;

            if (endpointNotFoundException != null)
                return true;

            ServerTooBusyException serverTooBusyException = exception as ServerTooBusyException;

            if (serverTooBusyException != null)
            {
                WebException webException = serverTooBusyException.InnerException as WebException;
                return IsEndpointDown(webException);
            }

            TimeoutException timeoutException = exception as TimeoutException;
            if (timeoutException != null)
                return true;

            DataServiceTransportException dataServiceTransportException = exception as DataServiceTransportException;

            if (dataServiceTransportException != null)
            {
                WebException webException = dataServiceTransportException.InnerException as WebException;
                return IsEndpointDown(webException);
            }

            DataServiceRequestException dataServiceRequestException = exception as DataServiceRequestException;

            if (dataServiceRequestException != null)
            {
                DataServiceClientException dataServiceClientException =
                    dataServiceRequestException.InnerException as DataServiceClientException;
                if (dataServiceClientException != null)
                {
                    WebException webException = dataServiceClientException.InnerException as WebException;
                    return IsEndpointDown(webException);
                }
            }

            return false;
        }

        /// <summary>
        /// Indicates whether the specified <see cref="WebException"/> is
        /// related to an endpoint not being available.
        /// </summary>
        /// <param name="webException"></param>
        /// <returns></returns>
        private static bool IsEndpointDown(WebException webException)
        {
            if (webException == null)
                return false;

            HttpStatusCode httpStatusCode = ((HttpWebResponse)webException.Response).StatusCode;
            return httpStatusCode == HttpStatusCode.NotFound ||
                   httpStatusCode == HttpStatusCode.ServiceUnavailable;
        }
    }
}