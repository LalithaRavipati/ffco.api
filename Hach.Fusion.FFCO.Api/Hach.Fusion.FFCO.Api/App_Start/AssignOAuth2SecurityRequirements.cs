using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Hach.Fusion.FFCO.Api
{
    /// <summary>
    /// Operation filter enabling the Swagger ui to use the web api.
    /// <see cref="http://knowyourtoolset.com/2015/08/secure-web-apis-with-swagger-swashbuckle-and-oauth2-part-2/"/>
    /// </summary>
    public class AssignOAuth2SecurityRequirements : IOperationFilter
    {
        /// <inheritDoc/>>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var actFilters = apiDescription.ActionDescriptor.GetFilterPipeline();
            var allowsAnonymous = actFilters.Select(f => f.Instance).OfType<OverrideAuthorizationAttribute>().Any();
            if (allowsAnonymous)
                return; // must be an anonymous method

            if (operation.security == null)
                operation.security = new List<IDictionary<string, IEnumerable<string>>>();

            var oAuthRequirements = new Dictionary<string, IEnumerable<string>>
            {
                { "oauth2", new List<string> {"FFAccessAPI"} }
                //{ "oauth2", new List<string> {"openid"} }
            };

            operation.security.Add(oAuthRequirements);
        }
    }
}