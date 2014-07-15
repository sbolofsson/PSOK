using System;

namespace PSOK.Kademlia.Services
{
    /// <summary>
    /// Class for working with <see cref="System.ServiceModel.Channels.Binding"/>s.
    /// </summary>
    internal static class Binding
    {
        /// <summary>
        /// Sets the timeout on the specified <see cref="System.ServiceModel.Channels.Binding"/>.
        /// </summary>
        /// <param name="binding"></param>
        internal static void SetTimeout(System.ServiceModel.Channels.Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException("binding");

            TimeSpan timeout = new TimeSpan(0, 0, 5);
            binding.CloseTimeout = timeout;
            binding.OpenTimeout = timeout;
            binding.ReceiveTimeout = timeout;
            binding.SendTimeout = timeout;
        }
    }
}
