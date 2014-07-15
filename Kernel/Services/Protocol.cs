namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Defines a transport protocol
    /// </summary>
    public enum Protocol
    {
        /// <summary>
        /// Defines no protocol.
        /// </summary>
        None = 0,

        /// <summary>
        /// Defines the http protocol.
        /// </summary>
        Http = 100,

        /// <summary>
        /// Defines the raw tcp protocol.
        /// </summary>
        Tcp = 200,
    }
}