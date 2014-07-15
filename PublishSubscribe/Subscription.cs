using System;
using System.Linq.Expressions;
using PSOK.Kernel.Caching;
using PSOK.Kernel.Reflection;
using PSOK.Kernel.Serialization;
using PSOK.PublishSubscribe.Messages;
using log4net;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// A subscription for communicating in the P2P network.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Subscription<T> : ISubscription where T : Message
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Subscription<T>));

        private string _conditionXml;

        private Expression<Func<T, bool>> _condition;

        /// <summary>
        /// The callback function to invoke when messages matching the <see cref="Subscription{T}" /> are published.
        /// </summary>
        public Action<T> Callback { get; set; }

        /// <summary>
        /// A predicate on the type T which must be fulfilled to receive a <see cref="Subscription{T}" />.
        /// </summary>
        public Expression<Func<T, bool>> Condition
        {
            get
            {
                if (_condition != null)
                    return _condition;

                if (_conditionXml == null)
                    return null;

                try
                {
                    return (_condition = Serializer.DeserializeExpression<Func<T, bool>>(_conditionXml));
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set { _condition = value; }
        }

        /// <summary>
        /// Indicates options on how to cache messages matching the <see cref="Subscription{T}" />.
        /// </summary>
        public ICachingOptions CachingOptions { get; set; }

        /// <summary>
        /// The type of the <see cref="Subscription{T}" />.
        /// </summary>
        Type ISubscription.Type
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// A unique key identifying the <see cref="Subscription{T}" />.
        /// </summary>
        string ISubscription.Key
        {
            get { return (this as ISubscription).Type.Key(); }
        }


        /// <summary>
        /// The entity set of the <see cref="Subscription{T}" />.
        /// </summary>
        string ISubscription.EntitySet { get; set; }

        /// <summary>
        /// Specifies whether subclasses of the type T should also be subscribed to.
        /// </summary>
        public bool IncludeSubclasses { get; set; }

        /// <summary>
        /// Checks whether the <see cref="Condition" /> of the <see cref="Subscription{T}" /> holds.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ISubscription.ConditionHolds(Message message)
        {
            try
            {
                return Condition == null || Condition.Compile()(message as T);
            }
            catch (Exception ex)
            {
                Log.Error("The condition function threw an exception. Callback will be not invoked.", ex);
            }
            return false;
        }

        /// <summary>
        /// Indicates whether the <see cref="Subscription{T}" /> has a condition.
        /// </summary>
        string ISubscription.ConditionXml
        {
            get
            {
                if (_condition != null)
                    _conditionXml = Serializer.SerializeExpression(_condition);

                return _conditionXml;
            }
            set { _conditionXml = value; }
        }

        /// <summary>
        /// Invokes the <see cref="Callback" /> of the <see cref="Subscription{T}" /> if the <see cref="Condition" /> holds.
        /// If the is no <see cref="Condition" /> the <see cref="Callback" /> will just be invoked.
        /// </summary>
        /// <param name="message"></param>
        void ISubscription.InvokeCallback(Message message)
        {
            try
            {
                if (Callback != null && (this as ISubscription).ConditionHolds(message))
                {
                    Callback(message as T);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Creates a copy of the <see cref="Subscription{T}" /> based on the given <see cref="Type" />.
        /// </summary>
        /// <param name="type">The type to use as the foundation for creating a copy.</param>
        /// <returns></returns>
        ISubscription ISubscription.MakeCopy(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            ISubscription thisSubscription = this;

            // Create subscription of the given type
            ISubscription subscription = Activator.CreateInstance(typeof(Subscription<>).MakeGenericType(type)) as ISubscription;

            if (subscription == null)
                throw new ArgumentException(string.Format("Could not create an instance of '{0}'.", type.AssemblyQualifiedName()));

            // Set private properties
            if (Callback != null)
            {
                PropertyHelper.SetProperty<Subscription<Message>>(subscription, x => x.Callback, Callback,
                    MethodHelper.GetUnderlyingMethod(Callback));
            }

            if (Condition != null)
            {
                PropertyHelper.SetProperty<Subscription<Message>>(subscription, x => x.Condition, Condition,
                    new ExpressionTransformer(thisSubscription.Type).Tranform(Condition));
            }

            // Set public properties
            subscription.CachingOptions = CachingOptions;
            subscription.IncludeSubclasses = IncludeSubclasses;
            subscription.EntitySet = thisSubscription.EntitySet;
            subscription.ConditionXml = thisSubscription.ConditionXml;

            return subscription;
        }

        /// <summary>
        /// Creates a serializable copy of the <see cref="Subscription{T}"/>.
        /// </summary>
        /// <returns></returns>
        public ISerializableSubscription MakeSerializable()
        {
            ISubscription subscription = this;

            return new SerializableSubscription
            {
                ConditionXml = subscription.ConditionXml,
                EntitySet = subscription.EntitySet,
                Type = subscription.Type.AssemblyQualifiedName()
            };
        }
    }
}