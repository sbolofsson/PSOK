using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using log4net;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Custom message inspector used to attach the custom header to WCF messages
    /// </summary>
    internal class LogMessageInspector : IClientMessageInspector, IDispatchMessageInspector
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LogMessageInspector));

        private readonly string _endpointName;

        public LogMessageInspector(string endpointName)
        {
            _endpointName = endpointName;
        }

        void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        /// <summary>
        /// Attach the custom header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        object IClientMessageInspector.BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            Log.DebugFormat("Endpoint: {0}. Sending data:\n{1}", _endpointName, request);
            return null;
        }

        /// <summary>
        /// Read the custom header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <param name="instanceContext"></param>
        /// <returns></returns>
        object IDispatchMessageInspector.AfterReceiveRequest(ref Message request, IClientChannel channel,
            InstanceContext instanceContext)
        {
            Log.DebugFormat("Endpoint: {0}. Received data:\n{1}",  _endpointName, request);
            return null;
        }

        void IDispatchMessageInspector.BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}
