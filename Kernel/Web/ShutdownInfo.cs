using System.Reflection;
using System.Web;

namespace PSOK.Kernel.Web
{
    /// <summary>
    /// Indicates information about an application shutdown.
    /// </summary>
    public class ShutdownInfo
    {
        /// <summary>
        /// The message reason of the shutdown.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The stack trace of the shutdown.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Retrieves shutdown information about an ongoing shutdown.
        /// </summary>
        /// <returns></returns>
        public static ShutdownInfo GetShutdownInfo()
        {
            try
            {
                HttpRuntime runtime = (HttpRuntime)typeof(HttpRuntime).InvokeMember("_theRuntime",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null);

                if (runtime == null)
                    return null;

                string shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);

                string shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);

                return new ShutdownInfo
                {
                    Message = shutDownMessage,
                    StackTrace = shutDownStack
                };
            }
            catch (System.Exception)
            {
                { }
            }
            return null;
        }
    }
}
