using System.Web.Mvc;

namespace PSOK.Kademlia.Web
{
    /// <summary>
    /// Configuration for filters.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Registers global filters.
        /// </summary>
        /// <param name="globalFilterCollection"></param>
        public static void RegisterGlobalFilters(GlobalFilterCollection globalFilterCollection)
        {
            globalFilterCollection.Add(new HandleErrorAttribute());
        }
    }
}