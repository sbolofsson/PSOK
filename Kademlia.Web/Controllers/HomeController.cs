using System.Web.Mvc;

namespace PSOK.Kademlia.Web.Controllers
{
    /// <summary>
    /// Default controller.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// The default method.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
    }
}