using System.Web.Mvc;

namespace Hach.Fusion.FFCO.Api.Controllers
{
    /// <summary>
    /// Home controller displaying a view that is displayed when this server is first launched.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Returns the home page when an index is requested.
        /// </summary>
        /// <returns>The home page view.</returns>
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
