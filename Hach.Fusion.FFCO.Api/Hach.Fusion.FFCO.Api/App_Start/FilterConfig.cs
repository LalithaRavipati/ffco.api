using System.Web.Mvc;

namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Configuration for filtering and post-processing requests.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Registers request filters.
        /// </summary>
        /// <param name="filters">Collection of filters to be registered.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
