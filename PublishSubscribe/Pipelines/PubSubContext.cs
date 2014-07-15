using System;
using System.Linq;
using System.ServiceModel.Web;
using System.Threading;
using PSOK.Kernel;
using PSOK.PublishSubscribe.Exceptions;
using PSOK.PublishSubscribe.Messages;
using PSOK.PublishSubscribe.Messages.Entities;
using PSOK.PublishSubscribe.Messages.Requests.DataRequest;
using log4net;

namespace PSOK.PublishSubscribe.Pipelines
{
    /// <summary>
    /// Provides access to publish-subscribe related information for the current pipeline execution context.
    /// </summary>
    public class PubSubContext
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PubSubContext));

        private static readonly ThreadLocal<PubSubContext> ThreadLocalPubSubContext = new ThreadLocal<PubSubContext>();

        static PubSubContext()
        {
            Application.OnExit += ThreadLocalPubSubContext.Dispose;
        }

        /// <summary>
        /// The current <see cref="PubSubContext" /> instance.
        /// </summary>
        public static PubSubContext Current
        {
            get
            {
                if (!ThreadLocalPubSubContext.IsValueCreated)
                    throw new PubSubContextException("A PubSubContext has not been created for the current thread. " +
                                                  "The PubSubContext.Current instance is only accessible in callbacks and publishes.");

                return ThreadLocalPubSubContext.Value;
            }
        }

        /// <summary>
        /// Indicates if a <see cref="PubSubContext.Current" /> instance has been created for the current thread.
        /// </summary>
        internal static bool IsInitialized
        {
            get { return ThreadLocalPubSubContext.IsValueCreated; }
        }

        /// <summary>
        /// Constructs a new <see cref="PubSubContext" />.
        /// </summary>
        private PubSubContext()
        {
        }

        /// <summary>
        /// The current <see cref="IPublish{T}" />.
        /// </summary>
        public IPublish<Message> Publish { get; private set; }

        /// <summary>
        /// Initializes a <see cref="PubSubContext.Current" /> instance for the current thread.
        /// </summary>
        private static void Initialize()
        {
            ThreadLocalPubSubContext.Value = new PubSubContext();
        }

        /// <summary>
        /// Disposes the <see cref="PubSubContext" /> and releases all resources held by this instance.
        /// </summary>
        internal static void Dispose()
        {
            ThreadLocalPubSubContext.Value = null;
        }

        /// <summary>
        /// Builds the current <see cref="PubSubContext" /> using information from the specified <see cref="IPublish{T}" />.
        /// </summary>
        /// <param name="publish"></param>
        internal static void BuildContext(IPublish<Message> publish)
        {
            try
            {
                Initialize();
                Current.Publish = publish;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Builds the current <see cref="PubSubContext" /> using information from the specified <see cref="WebOperationContext" />.
        /// </summary>
        /// <param name="webOperationContext"></param>
        internal static void BuildContext(WebOperationContext webOperationContext)
        {
            if (webOperationContext == null)
                throw new ArgumentNullException("webOperationContext");

            try
            {
                IncomingWebRequestContext request = webOperationContext.IncomingRequest;
                IPublish<Message> publish = ResolvePublish(request);
                BuildContext(publish);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Resolves the <see cref="IPublish{T}" /> type based on the specified <see cref="IncomingWebRequestContext" />.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static IPublish<Message> ResolvePublish(IncomingWebRequestContext request)
        {
            Publish<Message> publish = new Publish<Message>();
            foreach (string key in request.Headers.AllKeys)
            {
                string value = request.Headers[key];
                publish.Headers.Add(key, value);
            }

            publish.Message = ResolveRequest(request);

            return publish;
        }

        /// <summary>
        /// Resolves the <see cref="DataRequest" /> type based on the specified <see cref="IncomingWebRequestContext" />.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static DataRequest ResolveRequest(IncomingWebRequestContext request)
        {
            // Determine http verb - handle verb tunneling on post requests
            string method = (string.Equals(request.Method, "post", StringComparison.InvariantCultureIgnoreCase) &&
                request.Headers.AllKeys.Contains("X-HTTP-Method")
                ? request.Headers["X-HTTP-Method"]
                : request.Method).ToLower();

            switch (method)
            {
                case "post":
                    return new DataCreateRequest<Entity>();

                case "merge":
                case "put":
                    return new DataUpdateRequest<Entity>();

                case "delete":
                    return new DataDeleteRequest<Entity>();

                case "get":
                    return new DataGetRequest<Entity>();

                default:
                    return null;
            }
        }
    }
}