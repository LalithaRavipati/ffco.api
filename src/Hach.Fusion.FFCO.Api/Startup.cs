using System.Configuration;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.WebApi;
using Hach.Fusion.Core.Api.Controllers;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Data.Mapping;
using Hach.Fusion.FFCO.Api.AutofacModules;
using IdentityServer3.AccessTokenValidation;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Thinktecture.IdentityModel;

// Flags this class as needing to be called by OWIN at startup.
[assembly: OwinStartup(typeof(Hach.Fusion.FFCO.Api.Startup))]
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Code executed by OWIN at application startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configures the Web API services offered by this project.
        /// </summary>
        /// <param name="app">OWIN application builder that provides web services.</param>
        public void Configuration(IAppBuilder app)
        {
            log4net.Config.XmlConfigurator.Configure();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            app.UseCors(CorsOptions.AllowAll);

            var sbConnectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];
            GlobalHost.DependencyResolver.UseServiceBus(sbConnectionString, "ffcoapisignalr");

            var hubConfig = new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true,
                EnableJSONP = true
            };

            app.MapSignalR("/signalr", hubConfig);

            var controllerSelector = new ODataVersionControllerSelector(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            MappingManager.Initialize();

            var config = GlobalConfiguration.Configuration;

            // Configure Autofac
            var builder = new ContainerBuilder();

            builder.RegisterModule<ServiceModule>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();

            app.UseAutofacMiddleware(container);

            // Dependency for Web API
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            var authorityUrl = ConfigurationManager.AppSettings["IdServerUri"];
            app.UseIdentityServerBearerTokenAuthentication(
                new IdentityServerBearerTokenAuthenticationOptions
                {
                    Authority = authorityUrl,
                    RequiredScopes = new[] {"FFAccessAPI"}
                });

            app.ConfigureAuth();

            ClaimsAuthorization.CustomAuthorizationManager = new AuthorizationManager();

            var scope = container.BeginLifetimeScope();
            var service = scope.Resolve<ClaimsTransformer>();
            app.Use(typeof(ClaimsTransformationMiddleware), service);
        }
    }
}