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
using Hach.Fusion.FFCO.Api.DependencyInjection;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Owin;
using Thinktecture.IdentityModel;

// Flags this class as needing to be called by OWIN at startup.
[assembly: OwinStartup(typeof(Hach.Fusion.FFCO.Api.Startup))]
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Application startup code.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configuration performed during application start up.
        /// </summary>
        /// <param name="app">Application builder object being configured.</param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureLogging();

            RegisterAspClasses();

            var container = ConfigureDi(app);

            app.ConfigureAuth();

            ClaimsAuthorization.CustomAuthorizationManager = new AuthorizationManager();

            var scope = container.BeginLifetimeScope();
            var service = scope.Resolve<ClaimsTransformer>();
            app.UseClaimsTransformation(service);
        }

        private void ConfigureLogging()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        private void RegisterAspClasses()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var controllerSelector = new ODataVersionControllerSelector(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
        }

        private IContainer ConfigureDi(IAppBuilder app)
        {
            //MappingManager.Initialize();

            var builder = new ContainerBuilder();

            builder.RegisterModule<ServiceModule>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();

            app.UseAutofacMiddleware(container);

            // Dependency for Web API
            var config = GlobalConfiguration.Configuration;
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            var authorityUrl = ConfigurationManager.AppSettings["IdServerUri"];
            app.UseIdentityServerBearerTokenAuthentication(
                new IdentityServerBearerTokenAuthenticationOptions
                {
                    Authority = authorityUrl,
                    RequiredScopes = new[] { "FFAccessAPI" }
                });

            return container;
        }
    }
}
