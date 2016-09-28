using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Query;
using Hach.Fusion.Core.Api.Controllers;
using Hach.Fusion.Core.Api.OData;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Dtos;
using Swashbuckle.Swagger.Annotations;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Web API controller for managing parameter types.
    /// </summary>
    /// <remarks>
    /// All of the public methods below return an asynchronous task result containing information needed to create
    /// an API response message. Client applications using this API will not receive the task, but will instead receive
    /// a response message that originates from information in the task result. Since clients using this API, will
    /// see the XML comments in this class, the "return" fields below indicate the information returned to the
    /// client applications.
    /// </remarks>
    [EnableCors("*", "*", "*")]
    public class ParameterTypesController : FFAABaseController<ParameterTypeDto, Guid>
    {
        /// <summary>
        /// Default constructor for the <see cref="ParametersController"/> class taking OData helper and repository facade arguments.
        /// </summary>
        /// <param name="oDataHelper">Helper that provides OData utilities to manage requests.</param>
        /// <param name="facade">Facade for the repository used to retrieve location type data.</param>
        public ParameterTypesController(IODataHelper oDataHelper, IFacade<ParameterTypeDto, Guid> facade) 
            : base(oDataHelper)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            _facade = facade;
        }

        /// <summary>
        /// Retrieves a queryable list of parameter types.
        /// </summary>
        /// <param name="queryOptions">OData query options that provide for sorting and filtering.</param>
        /// <returns>
        /// A list of DTOs for the entity types that satisfy query option criteria.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/ParameterTypes
        /// </example>
        [FFSEAuthorize(PermissionAction.Read, PermissionResource.ParameterType)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(ParameterTypeDto))]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<ParameterTypeDto> queryOptions)
        {
            var results = await _facade.Get(queryOptions);
            return Query(results);
        }

        /// <summary>
        /// Returns the parameter type with the specified ID.
        /// </summary>
        /// <param name="key">Id that identifies the entity to be retrieved.</param>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// The DTO for the indicated entity.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/ParameterTypes(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        [FFSEAuthorize(PermissionAction.Read, PermissionResource.ParameterType)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(ParameterTypeDto))]
        public async Task<IHttpActionResult> Get([FromODataUri] Guid key, ODataQueryOptions<ParameterTypeDto> queryOptions)
        {
            var results = await _facade.Get(key);
            return Query(results);
        }

        /// <summary>
        /// Gets a single property of a specified parameter type.
        /// </summary>
        /// <param name="key">Id of the entity whose property is to be retrieved.</param>
        /// <param name="propertyName">Name of the property to be retrieved.</param>
        /// <returns>
        /// The value of the indicated entity property.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/ParameterTypes(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)/Description
        /// </example>
        [FFSEAuthorize(PermissionAction.Read, PermissionResource.ParameterType)]
        [HttpGet]
        public async Task<IHttpActionResult> GetProperty([FromODataUri] Guid key, string propertyName)
        {
            var result = await _facade.GetProperty(key, propertyName);
            return Query(result);
        }
    }
}