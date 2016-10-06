using System.Web;
using Hach.Fusion.Core.CommonActorSystem;

namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// ASP.NET application object.
    /// </summary>
    public class WebApiApplication : HttpApplication
    {
        /// <summary>
        /// Executes when the application is shutting down.
        /// </summary>
        protected void Application_End()
        {
            ActorSystemReferences.Terminate().Wait();
        }
    }
}