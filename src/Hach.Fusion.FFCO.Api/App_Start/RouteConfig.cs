using System.Web.Mvc;
using System.Web.Routing;

namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Configures routing of requests.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Registers request routers.
        /// </summary>
        /// <param name="routes">Routes to be registered.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
