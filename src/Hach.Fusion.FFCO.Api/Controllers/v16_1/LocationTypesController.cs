using Hach.Fusion.Core.Api.Controllers;
using Hach.Fusion.Core.Api.OData;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Dtos;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Query;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Web API controller for managing Location Types.
    /// </summary>
    /// <remarks>
    /// All of the public methods below return an asynchronous task result containing information needed to create
    /// an API response message. Client applications using this API will not receive the task, but will instead receive
    /// a response message that originates from information in the task result. Since clients using this API, will
    /// see the XML comments in this class, the "return" fields below indicate the information returned to the
    /// client applications.
    /// </remarks>
    public class LocationTypesController
        : ControllerWithCruModelsBase<LocationTypeBaseDto, LocationTypeBaseDto, LocationTypeQueryDto, Guid>
    {
        /// <summary>
        /// Default constructor for the <see cref="LocationTypesController"/> class taking OData helper and repository facade arguments.
        /// </summary>
        /// <param name="oDataHelper">Helper that provides OData utilities to manage requests.</param>
        /// <param name="facade">Facade for the repository used to persist Location Type data.</param>
        public LocationTypesController(IODataHelper oDataHelper, IFacadeWithCruModels<LocationTypeBaseDto, LocationTypeBaseDto, LocationTypeQueryDto, Guid> facade) 
            : base(oDataHelper)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            Facade = facade;
        }

        /// <summary>
        /// Retrieves a queryable list of Location Types.
        /// </summary>
        /// <param name="queryOptions">OData query options that provide for sorting and filtering.</param>
        /// <returns>
        /// A list of DTOs for the Location Types that satisfy query option criteria.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/LocationTypes
        /// </example>
        /// <include file='XmlDocumentation/LocationTypesController.doc' path='LocationTypesController/Methods[@name="GetAll"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery(MaxExpansionDepth=Constants.DefaultMaxExpansionDepth)]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(LocationTypeQueryDto))]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<LocationTypeQueryDto> queryOptions)
        {
            var results = await Facade.Get(queryOptions);
            return Query(results);
        }

        /// <summary>
        /// Retrieves the Location Type with the specified ID.
        /// </summary>
        /// <param name="key">ID that identifies the Location Type to be retrieved.</param>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// The DTO for the indicated Location Type.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/LocationTypes(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/LocationTypesController.doc' path='LocationTypesController/Methods[@name="GetOne"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery(MaxExpansionDepth=Constants.DefaultMaxExpansionDepth)]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(LocationTypeQueryDto))]
        public async Task<IHttpActionResult> Get([FromODataUri] Guid key, ODataQueryOptions<LocationTypeQueryDto> queryOptions)
        {
            var results = await Facade.Get(key);
            return Query(results);
        }
    }
}