using System;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Hach.Fusion.Core.Api.Controllers;
using Hach.Fusion.Core.Api.Handlers;
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Dtos.Dashboards;
using Hach.Fusion.FFCO.Core.Dtos.LimitTypes;
using Microsoft.OData.Edm;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Application;
using Swashbuckle.OData;

namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Configures the web service by registering needed components and establishing an Entity
    /// Data Model (EDM).
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers Web API services.
        /// </summary>
        /// <param name="config">Configuration information for the server.</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Enable Cross-origin Resource Sharing (CORS)
            config.EnableCors();

            // Web API routes
            config.MapHttpAttributeRoutes();

            var edmModel = GetImplicitEdm();
            var conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting(config, edmModel);
            conventions.Insert(0, new CustomPropertyRoutingConvention());

            config.MapODataServiceRoute(
                "V161RouteVersioning",
                "odata/v16.1",
                edmModel,
                new DefaultODataPathHandler(),
                conventions);

            var controllerSelector =
                config.Services.GetService(typeof(IHttpControllerSelector)) as ODataVersionControllerSelector;

            controllerSelector?.RouteVersionSuffixMapping.Add("V161RouteVersioning", "v16_1");

            // Automatically change the Pascal casing standard in C# MVC to Camal Case Standard used in JavaScript
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;

            config.MessageHandlers.Add(new LogRequestAndResponseHandler());
            config.EnableCaseInsensitive(true);

            ConfigureSwagger(config);
        }

        /// <summary>
        /// Builds an Entity Data Model used by the message router and controllers.
        /// </summary>
        /// <returns>An Entity Data Model.</returns>
        /// <remarks>
        /// DTOs need an EntitySet so that the OData controller can 'magically' parse the json into an object.
        /// Only ONE DTO can be bound per table, so a BaseDto object is needed. 
        /// </remarks>
        /// 
        private static IEdmModel GetImplicitEdm()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<LocationBaseDto>("Locations");
            builder.EntitySet<LocationLogEntryBaseDto>("LocationLogEntries");
            builder.EntitySet<LocationTypeCommandDto>("LocationTypes");
            builder.EntitySet<DashboardBaseDto>("Dashboards");
            builder.EntitySet<DashboardOptionBaseDto>("DashboardOptions");

            builder.EntitySet<UnitTypeQueryDto>("UnitTypes");
            builder.EntitySet<UnitTypeGroupQueryDto>("UnitTypeGroups");
            builder.EntitySet<ParameterTypeDto>("ParameterTypes");
            builder.EntitySet<ParameterDto>("Parameters");
            builder.EntitySet<ParameterValidRangeQueryDto>("ParameterValidRanges");
            builder.EntitySet<LimitTypeQueryDto>("LimitTypes");
            builder.EntitySet<ChemicalFormTypeQueryDto>("ChemicalFormTypes");

            builder.EnableLowerCamelCase();

            return builder.GetEdmModel();
        }        

        /// <summary>
        /// Configure Swagger API documentation.
        /// </summary>
        /// <param name="config">Configuration information for the server.</param>
        private static void ConfigureSwagger(HttpConfiguration config)
        {
            var version = typeof(WebApiApplication).Assembly.GetName().Version;

            // Configure swagger for api documentation.
            var authority = ConfigurationManager.AppSettings["IdServerUri"];
            // https://github.com/rbeauchamp/Swashbuckle.OData
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v16_1", "FFCO API")
                        .Description($"Version {version}")
                        .Contact(cc => cc
                            .Name("Hach Company")
                            .Url("www.hach.com"));
                    c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c));
                    c.DescribeAllEnumsAsStrings();
                    c.IncludeXmlComments(GetXmlDocumentationFilename());
                    c.UseFullTypeNameInSchemaIds();
                    c.OAuth2("oauth2")
                        .Description("OAuth2 Implicit Grant")
                        .Flow("implicit")
                        .AuthorizationUrl(authority + "/connect/authorize")
                        .TokenUrl(authority + "/connect/token")
                        .Scopes(scopes =>
                        {
                            scopes.Add("FFAccessAPI", "Scope required to access all FFCO API endpoints.");
                        });
                    c.OperationFilter<AssignOAuth2SecurityRequirements>();
                })
                .EnableSwaggerUi("api/{*assetPath}", u =>
                {
                    u.InjectStylesheet(typeof(WebApiApplication).Assembly, "Hach.Fusion.FFCO.Api.Resources.SwaggerStyle.css");
                    u.EnableOAuth2Support("Swagger.ImplicitFlow", "dummyRealm", "Swagger UI");
                    u.CustomAsset("index", typeof(WebApiApplication).Assembly, "Hach.Fusion.FFCO.Api.Resources.SwaggerDefault");
                    u.CustomAsset("images/logo_small-png", typeof(WebApiApplication).Assembly, "Hach.Fusion.FFCO.Api.Resources.HachLogo.png");
                    u.CustomAsset("images/HachTabLogo", typeof(WebApiApplication).Assembly, "Hach.Fusion.FFCO.Api.Resources.HachTabLogo.png");

                    // Disable swagger validator that shows error when deployed to production
                    // http://stackoverflow.com/a/31734825/9840
                    u.DisableValidator();
                });
        }

        /// <summary>
        /// Gets the XML documentation filename. (XML documentation must be enabled in the project properties).
        /// </summary>
        /// <returns>The XML documentation filename.</returns>
        private static string GetXmlDocumentationFilename()
        {
            return $@"{AppDomain.CurrentDomain.BaseDirectory}bin\Hach.Fusion.FFCO.Api.XML";
        }
    }
}
