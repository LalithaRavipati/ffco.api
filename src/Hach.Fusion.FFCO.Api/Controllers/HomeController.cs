using System.Web.Mvc;

namespace Hach.Fusion.FFCO.Api.Controllers
{
    /// <summary>
    /// Home page controller for API.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
