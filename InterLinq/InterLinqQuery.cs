using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using InterLinq.Types;

namespace InterLinq
{
    /// <summary>
    /// Base Class for InterLinq Queries.
    /// </summary>
    [Serializable]
    [DataContract]
    public abstract class InterLinqQueryBase
    {

        #region Properties

        #region Property ElementType

        /// <summary>
        /// See <see cref="Type">Element Type</see> of the <see cref="Expression"/>.
        /// </summary>
        [NonSerialized]
        protected Type ElementTypeInstance;
        /// <summary>
        /// Gets or sets a <see cref="Type">Element Type</see> of the <see cref="Expression"/>.
        /// </summary>
        public abstract Type ElementType { get; }

        #endregion

        #region Property InterLinqElementType

        /// <summary>
        /// See <see cref="InterLinqType">InterLINQ Element Type</see> of the <see cref="Expression"/>.
        /// </summary>
        protected InterLinqType ElementInterLinqTypeInstance;
        /// <summary>
        /// Gets or sets a <see cref="InterLinqType">InterLINQ Element Type</see> of the <see cref="Expression"/>.
        /// </summary>
        [DataMember]
        public InterLinqType ElementInterLinqType
        {
            get { return ElementInterLinqTypeInstance; }
            set { ElementInterLinqTypeInstance = value; }
        }

        #endregion

        #endregion

    }

    /// <summary>
    /// Standard implementation of an InterLinqQuery.
    /// </summary>
    /// <typeparam name="T">The type of the content of the data source.</typeparam>
    /// <seealso cref="InterLinqQueryBase"/>
    [Serializable]
    [DataContract]
    public class InterLinqQuery<T> : InterLinqQueryBase, IOrderedQueryable<T>
    {

        #region Fields

        [NonSerialized]
        private IQueryProvider _provider;
        [NonSerialized]
        private Expression _expression;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes this Query with the arguments.
        /// </summary>
        /// <remarks>
        /// This constructor creates a <see cref="ConstantExpression"/>.
        /// The arguments will be checked. These exceptions will be thrown...
        ///     ... when "provider" is null:    <see cref="ArgumentNullException"/>
        /// </remarks>
        /// <param name="provider"><see cref="IQueryProvider"/> to set.</param>
        public InterLinqQuery(IQueryProvider provider)
        {
            Initialize(provider, Expression.Constant(this));
        }

        /// <summary>
        /// Initializes this Query with the arguments.
        /// </summary>
        /// <remarks>
        /// The arguments will be checked. These exceptions will be thrown...
        /// <list type="list">
        ///     <listheader>
        ///         <term>Condition</term>
        ///         <description>Thrown Exception</description>
        ///     </listheader>
        ///     <item>
        ///         <term>... when "provider" is null</term>
        ///         <description><see cref="ArgumentNullException"/></description>
        ///     </item>
        ///     <item>
        ///         <term>... when "expression" is null</term>
        ///         <description><see cref="ArgumentNullException"/></description>
        ///     </item>
        ///     <item>
        ///         <term>... when "expression" is not assignable from <see cref="IQueryable{T}"/></term>
        ///         <description><see cref="ArgumentException"/></description>
        ///     </item>
        /// </list>                                  
        /// </remarks>
        /// <param name="provider"><see cref="IQueryProvider"/> to set.</param>
        /// <param name="expression"><see cref="Expression"/> to set.</param>
        public InterLinqQuery(IQueryProvider provider, Expression expression)
        {
            Initialize(provider, expression);
        }

        /// <summary>
        /// Checks the method arguments and initializes this Query object.
        /// </summary>
        /// <param name="iQueryProvider"><see cref="IQueryProvider"/> to set.</param>
        /// <param name="expr"><see cref="Expression"/> to set.</param>
        private void Initialize(IQueryProvider iQueryProvider, Expression expr)
        {
            if (iQueryProvider == null)
            {
                throw new ArgumentNullException("iQueryProvider");
            }
            if (expr == null)
            {
                throw new ArgumentNullException("expr");
            }

            if (!typeof(IQueryable<T>).IsAssignableFrom(expr.Type))
            {
                throw new ArgumentException("expr");
            }
            _provider = iQueryProvider;
            _expression = expr;
            ElementTypeInstance = typeof(T);
            ElementInterLinqTypeInstance = InterLinqTypeSystem.Instance.GetInterLinqVersionOf<InterLinqType>(ElementTypeInstance);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see langword="string"/> that represents this instance.
        /// </summary>
        /// <remarks>
        /// The following <see langword="string"/> is returned:
        /// <c>Type&lt;GenericArgumentType&gt;</c>
        /// </remarks>
        /// <returns>A <see langword="string"/> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0}<{1}>", GetType().Name, typeof(T));
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that iterates through the returned result
        /// of this query.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> object that can be used to iterate 
        /// through the returned result of this query.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            IEnumerable retrievedObjects = (IEnumerable)_provider.Execute(_expression);
            object returnValue = TypeConverter.ConvertFromSerializable(typeof(IEnumerable<T>), retrievedObjects);
            return ((IEnumerable<T>)returnValue).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> that iterates through the returned result
        /// of this query.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate 
        /// through the returned result of this query.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IQueryable Members

        /// <summary>
        /// Gets the type of the element(s) that are returned when the 
        /// <see cref="Expression"/> tree associated with this instance of 
        /// <see cref="IQueryable"/> is executed.
        /// </summary>
        /// <remarks>
        /// A <see cref="Type"/> that represents the type of the element(s) that are returned
        /// when the <see cref="Expression"/> tree associated with this object is executed.
        /// </remarks>
        public override Type ElementType
        {
            get
            {
                if (ElementTypeInstance == null && ElementInterLinqTypeInstance != null)
                {
                    ElementTypeInstance = (Type)ElementInterLinqTypeInstance.GetClrVersion();
                }
                // ReSharper disable AssignNullToNotNullAttribute
                return ElementTypeInstance;
                // ReSharper restore AssignNullToNotNullAttribute
            }
        }

        /// <summary>
        /// Gets the <see cref="IQueryProvider"/> that is associated with this data source.
        /// </summary>
        /// <remarks>
        /// The <see cref="IQueryProvider"/> that is associated with this data source.
        /// </remarks>
        public IQueryProvider Provider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Gets the <see cref="Expression"/> tree that is associated with the instance of 
        /// <see cref="IQueryable"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Expression"/> that is associated with this instance of 
        /// <see cref="IQueryable"/>.
        /// </remarks>
        public Expression Expression
        {
            get { return _expression; }
        }

        #endregion

    }
}
