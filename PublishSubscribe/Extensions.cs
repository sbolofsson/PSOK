using System;
using PSOK.Kademlia;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Messages;
using PSOK.PublishSubscribe.Reports;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts the <see cref="IContact"/> to an <see cref="ISubscription"/>.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static ISubscription ToSubscription(this IContact contact)
        {
            ISerializableSubscription serializableSubscription = contact.Data as ISerializableSubscription;
            if (serializableSubscription == null)
                return null;
            ISubscription subscription = serializableSubscription.ToSubscription();
            return subscription;
        }

        /// <summary>
        /// Checks if the condition for the associated <see cref="ISubscription"/> holds.
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ConditionHolds(this IContact contact, Message message)
        {
            ISubscription subscription = contact.ToSubscription();
            return subscription == null || subscription.ConditionHolds(message);
        }

        /// <summary>
        /// The public debug URL of a <see cref="Subscription{T}" />. Is used for <see cref="IStatusReport" />s.
        /// </summary>
        public static string DebugUrl(this IContact contact)
        {
            return string.Format("{0}/kademlia/{1}/debug",
            contact.BaseUrl,
            contact.NodeId);
        }

        /// <summary>
        /// The public OData service URL of a <see cref="Subscription{T}" />.
        /// </summary>
        public static string DataContextUrl(this IContact contact)
        {
            // OData urls only works over http and https
            string dataUrl = string.Format("{0}/kademlia/{1}/data",
                contact.BaseUrl,
                contact.NodeId);
            UriBuilder uriBuilder = new UriBuilder(dataUrl)
            {
                Scheme = Transport.EnableSsl ? "https" : "http",
                Port = Transport.EnableSsl ? Transport.DefaultPort : Transport.HttpPort
            };
            return uriBuilder.Uri.ToString();
        }

        /// <summary>
        /// The public broker URL of a <see cref="Subscription{T}" />.
        /// </summary>
        public static string PeerUrl(this IContact contact)
        {
            return string.Format("{0}/kademlia/{1}/peer",
            contact.BaseUrl,
            contact.NodeId);
        }
    }
}
