using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Query;
using Hach.Fusion.Core.Api.Controllers;
using Hach.Fusion.Core.Api.OData;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Core.Dtos.Dashboards;
using Swashbuckle.Swagger.Annotations;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Web API controller for managing DashboardOptions.
    /// </summary>
    public class DashboardOptionsController
        : ControllerWithCruModelsBase<DashboardOptionCommandDto, DashboardOptionCommandDto, DashboardOptionQueryDto, Guid>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oDataHelper">Helper that provides OData utilities to manage requests.</param>
        /// <param name="facade">Facade for the repository used to persist data.</param>
        public DashboardOptionsController(IODataHelper oDataHelper,
            IFacadeWithCruModels<DashboardOptionCommandDto, DashboardOptionCommandDto, DashboardOptionQueryDto, Guid> facade) 
            : base(oDataHelper)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            Facade = facade;
        }

        /// <summary>
        /// Retrieves a queryable list of DashboardOptions.
        /// </summary>
        /// <param name="queryOptions">OData query options that provide for sorting and filtering.</param>
        /// <returns>
        /// A list of DTOs that satisfy query option criteria.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/DashboardOptions
        /// </example>
        /// <include file='XmlDocumentation/DashboardOptionsController.doc' path='DashboardOptionsController/Methods[@name="GetAll"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(DashboardOptionQueryDto))]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<DashboardOptionQueryDto> queryOptions)
        {
            var results = await Facade.Get(queryOptions);
            return Query(results);
        }

        /// <summary>
        /// Retrieves the DashboardOption with the specified ID.
        /// </summary>
        /// <param name="key">ID that identifies the entity to be retrieved.</param>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// The DTO for the indicated entity.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/DashboardOptions(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/DashboardOptionsController.doc' path='DashboardOptionsController/Methods[@name="GetOne"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(DashboardOptionQueryDto))]
        public async Task<IHttpActionResult> Get([FromODataUri] Guid key, ODataQueryOptions<DashboardOptionQueryDto> queryOptions)
        {
            var results = await Facade.Get(key);
            return Query(results);
        }

        /// <summary>
        /// Creates a DashboardOption.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) of the entity to be created.</param>
        /// <returns>
        /// The DTO for the newly created entity.
        /// </returns>
        /// <example>
        /// POST: ~/odata/v16.1/DashboardOptions
        /// </example>
        /// <include file='XmlDocumentation/DashboardOptionsController.doc' path='DashboardOptionsController/Methods[@name="Post"]/*'/>
        [FFSEAuthorize(PermissionAction.Create)]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Created, null, typeof(CommandResult<DashboardOptionCommandDto, Guid>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(CommandResult<DashboardOptionCommandDto, Guid>))]
        public async Task<IHttpActionResult> Post(DashboardOptionCommandDto dto)
        {
            var result = await Facade.Create(dto);
            return Command(result);
        }

        /// <summary>
        /// Replaces the specified properties of the indicated DashboardOption.
        /// </summary>
        /// <param name="key">Key that uniquely identifies the entity to be edited.</param>
        /// <param name="delta">Delta for the updated entity properties.</param>
        /// <returns>
        /// If successful, this method always returns "No Content".
        /// </returns>
        /// <example>
        /// PATCH: ~/odata/v16.1/DashboardOptions(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// MERGE: ~/odata/v16.1/DashboardOptions(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/DashboardOptionsController.doc' path='DashboardOptionsController/Methods[@name="Patch"]/*'/>
        [FFSEAuthorize(PermissionAction.Update)]
        [AcceptVerbs("PATCH", "MERGE")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(CommandResult<DashboardOptionCommandDto, Guid>))]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(CommandResult<DashboardOptionCommandDto, Guid>))]
        public async Task<IHttpActionResult> Patch([FromODataUri] Guid key, Delta<DashboardOptionCommandDto> delta)
        {
            var result = await Facade.Update(key, delta);
            return Command(result);
        }

        /// <summary>
        /// Deletes the DashboardOption with the specified ID.
        /// </summary>
        /// <param name="key">ID of the entity to be deleted.</param>
        /// <returns>
        /// Status code indicating whether the operation was successful or why it failed.
        /// </returns>
        /// <example>
        /// DELETE: ~/odata/v16.1/DashboardOptions(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/DashboardOptionsController.doc' path='DashboardOptionsController/Methods[@name="Delete"]/*'/>
        [FFSEAuthorize(PermissionAction.Delete)]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(CommandResult<DashboardOptionQueryDto, Guid>))]
        public async Task<IHttpActionResult> Delete([FromODataUri] Guid key)
        {
            var result = await Facade.Delete(key);
            return Command(result);
        }
    }
}