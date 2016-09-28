using System;
using System.Configuration;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Hach.Fusion.Core.Api.Controllers;
using Hach.Fusion.Core.Api.Handlers;
using Hach.Fusion.Core.Extensions;
using Hach.Fusion.FFCO.Api.Controllers.v16_1;
using Hach.Fusion.FFCO.Dtos;
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
        /// <param name="config"></param>
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

            // Configure swagger for api documentation.
            var authority = ConfigurationManager.AppSettings["IdServerUri"];
            // https://github.com/rbeauchamp/Swashbuckle.OData
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v16_1", "Hach.Fusion.FFCO.API Documentation")
                        .Description("")
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
                .EnableSwaggerUi(u =>
                {
                    u.InjectStylesheet(typeof(LocationsController).Assembly, "Hach.Fusion.FFCO.Api.Resources.SwaggerStyle.css");
                    u.EnableOAuth2Support("Swagger.ImplicitFlow", "dummyRealm", "Swagger UI");
                });
        }

        /// <summary>
        /// Builds an Entity Data Model used by the message router and controllers.
        /// </summary>
        /// <returns>An Entity Data Model.</returns>
        private static IEdmModel GetImplicitEdm()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<LocationBaseDto>("Locations");
            builder.EntitySet<LocationTypeQueryDto>("LocationTypes");
            builder.EntitySet<UnitTypeQueryDto>("UnitTypes");
            builder.EntitySet<UnitTypeGroupQueryDto>("UnitTypeGroups");

            builder.EnableLowerCamelCase();

            return builder.GetEdmModel();
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
