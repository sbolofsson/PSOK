using System;
using System.Linq;

namespace InterLinq.Communication
{
    /// <summary>
    /// Client implementation of the <see cref="InterLinqQueryHandler"/>.
    /// </summary>
    /// <seealso cref="InterLinqQueryHandler"/>
    /// <seealso cref="IQueryHandler"/>
    public class ClientQueryHandler : InterLinqQueryHandler
    {

        #region Properties

        /// <summary>
        /// Gets the <see cref="IQueryProvider"/>.
        /// </summary>
        /// <seealso cref="InterLinqQueryHandler.QueryProvider"/>
        public override IQueryProvider QueryProvider
        {
            get { return new ClientQueryProvider(QueryRemoteHandler); }
        }

        /// <summary>
        /// <see cref="IQueryRemoteHandler"/> instance.
        /// </summary>
        protected IQueryRemoteHandler QueryRemoteHandlerInstance;
        /// <summary>
        /// Gets the <see cref="IQueryRemoteHandler"/>;
        /// </summary>
        public virtual IQueryRemoteHandler QueryRemoteHandler
        {
            get
            {
                if (QueryRemoteHandlerInstance == null)
                {
                    Connect();
                }
                return QueryRemoteHandlerInstance;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ClientQueryHandler() { }

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="queryRemoteHandlerInstance"><see cref="IQueryRemoteHandler"/> to communicate with the server.</param>
        public ClientQueryHandler(IQueryRemoteHandler queryRemoteHandlerInstance)
        {
            QueryRemoteHandlerInstance = queryRemoteHandlerInstance;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connects to the server.
        /// </summary>
        public virtual void Connect()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
