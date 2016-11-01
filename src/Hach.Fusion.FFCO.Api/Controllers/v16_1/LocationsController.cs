using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.Http.Cors;
using System.Web.OData.Query;
using Hach.Fusion.Core.Api.Controllers;
using Hach.Fusion.Core.Api.OData;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Core.Dtos;
using Swashbuckle.Swagger.Annotations;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Web API controller for managing Locations.
    /// </summary>
    /// <remarks>
    /// All of the public methods below return an asynchronous task result containing information needed to create
    /// an API response message. Client applications using this API will not receive the task, but will instead receive
    /// a response message that originates from information in the task result. Since clients using this API, will
    /// see the XML comments in this class, the "return" fields below indicate the information returned to the
    /// client applications.
    /// </remarks>
    [EnableCors("*", "*", "*")]
    public class LocationsController
        : ControllerWithCruModelsBase<LocationCommandDto, LocationCommandDto, LocationQueryDto, Guid>
    {
        /// <summary>
        /// Default constructor for the <see cref="LocationsController"/> class taking OData helper and repository facade arguments.
        /// </summary>
        /// <param name="oDataHelper">Helper that provides OData utilities to manage requests.</param>
        /// <param name="facade">Facade for the repository used to persist Location data.</param>
        public LocationsController(IODataHelper oDataHelper, IFacadeWithCruModels<LocationCommandDto, LocationCommandDto, LocationQueryDto, Guid> facade) 
            : base(oDataHelper)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            Facade = facade;
        }

        /// <summary>
        /// Retrieves a queryable list of Locations.
        /// </summary>
        /// <param name="queryOptions">OData query options that provide for sorting and filtering.</param>
        /// <returns>
        /// A list of DTOs for the Locations that satisfy query option criteria.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/Locations
        /// </example>
        /// <include file='XmlDocumentation/LocationsController.doc' path='LocationsController/Methods[@name="GetAll"]/*'/>
        //[FFSEAuthorize(PermissionAction.Read, PermissionResource.Location)]
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(LocationQueryDto))]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<LocationQueryDto> queryOptions)
        {
            var results = await Facade.Get(queryOptions);
            return Query(results);
        }

        /// <summary>
        /// Retrieves the Location with the specified ID.
        /// </summary>
        /// <param name="key">ID that identifies the Location to be retrieved.</param>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// The DTO for the indicated Location.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/Locations(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/LocationsController.doc' path='LocationsController/Methods[@name="GetOne"]/*'/>
        //[FFSEAuthorize(PermissionAction.Read, PermissionResource.Location)]
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(LocationQueryDto))]
        public async Task<IHttpActionResult> Get([FromODataUri] Guid key, ODataQueryOptions<LocationQueryDto> queryOptions)
        {
            var results = await Facade.Get(key);
            return Query(results);
        }

        /// <summary>
        /// Creates a Location.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) of the Location to be created.</param>
        /// <returns>
        /// The DTO for the newly created Location.
        /// </returns>
        /// <example>
        /// POST: ~/odata/v16.1/Locations
        /// </example>
        /// <include file='XmlDocumentation/LocationsController.doc' path='LocationsController/Methods[@name="Post"]/*'/>
        //[FFSEAuthorize(PermissionAction.Create, PermissionResource.Location)]
        [FFSEAuthorize(PermissionAction.Create)]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Created, null, typeof(CommandResult<LocationCommandDto, Guid>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(CommandResult<LocationCommandDto, Guid>))]
        public async Task<IHttpActionResult> Post(LocationCommandDto dto)
        {
            var result = await Facade.Create(dto);
            return Command(result);
        }

        /// <summary>
        /// Replaces the specified properties of the indicated Location.
        /// </summary>
        /// <param name="key">Key that uniquely identifies the Location to be edited.</param>
        /// <param name="delta">Delta for the updated Location properties.</param>
        /// <returns>
        /// If successful, this method always returns "No Content".
        /// </returns>
        /// <example>
        /// PATCH: ~/odata/v16.1/Locations(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// MERGE: ~/odata/v16.1/Locations(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/LocationsController.doc' path='LocationsController/Methods[@name="Patch"]/*'/>
        //[FFSEAuthorize(PermissionAction.Update, PermissionResource.Location)]
        [FFSEAuthorize(PermissionAction.Update)]
        [AcceptVerbs("PATCH", "MERGE")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(CommandResult<LocationCommandDto, Guid>))]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(CommandResult<LocationCommandDto, Guid>))]
        public async Task<IHttpActionResult> Patch([FromODataUri] Guid key, Delta<LocationCommandDto> delta)
        {
            var result = await Facade.Update(key, delta);
            return Command(result);
        }

        /// <summary>
        /// Deletes the Location with the specified ID.
        /// </summary>
        /// <param name="key">ID of the Location to be deleted.</param>
        /// <returns>
        /// Status code indicating whether the operation was successful or why it failed.
        /// </returns>
        /// <example>
        /// DELETE: ~/odata/v16.1/Locations(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/LocationsController.doc' path='LocationsController/Methods[@name="Delete"]/*'/>
        //[FFSEAuthorize(PermissionAction.Delete, PermissionResource.Location)]
        [FFSEAuthorize(PermissionAction.Delete)]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(CommandResult<LocationQueryDto, Guid>))]
        public async Task<IHttpActionResult> Delete([FromODataUri] Guid key)
        {
            var result = await Facade.Delete(key);
            return Command(result);
        }
    }
}