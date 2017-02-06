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
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Core.Dtos;
using Swashbuckle.Swagger.Annotations;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Web API controller for managing Unit Type Groups.
    /// </summary>
    /// <remarks>
    /// All of the public methods below return an asynchronous task result containing information needed to create
    /// an API response message. Client applications using this API will not receive the task, but will instead receive
    /// a response message that originates from information in the task result. Since clients using this API, will
    /// see the XML comments in this class, the "return" fields below indicate the information returned to the
    /// client applications.
    /// </remarks>
    public class UnitTypeGroupsController
        : ControllerWithCruModelsBase<UnitTypeGroupQueryDto, UnitTypeGroupQueryDto, UnitTypeGroupQueryDto, Guid>
    {
        /// <summary>
        /// Default constructor for the <see cref="UnitTypeGroupsController"/> class taking OData helper and repository facade arguments.
        /// </summary>
        /// <param name="oDataHelper">Helper that provides OData utilities to manage requests.</param>
        /// <param name="facade">Facade for the repository used to persist Unit Type Group data.</param>
        public UnitTypeGroupsController(IODataHelper oDataHelper, IFacadeWithCruModels<UnitTypeGroupQueryDto, UnitTypeGroupQueryDto, UnitTypeGroupQueryDto, Guid> facade) 
            : base(oDataHelper)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            Facade = facade;
        }

        /// <summary>
        /// Retrieves a queryable list of Unit Type Group.
        /// </summary>
        /// <param name="queryOptions">OData query options that provide for sorting and filtering.</param>
        /// <returns>
        /// A list of DTOs for the Unit Type Groups that satisfy query option criteria.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/UnitTypeGroups
        /// </example>
        /// <include file='XmlDocumentation/UnitTypeGroupsController.doc' path='UnitTypeGroupsController/Methods[@name="GetAll"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery(MaxExpansionDepth=Constants.DefaultMaxExpansionDepth)]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(UnitTypeGroupQueryDto))]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<UnitTypeGroupQueryDto> queryOptions)
        {
            var results = await Facade.Get(queryOptions);
            return Query(results);
        }

        /// <summary>
        /// Retrieves the Unit Type Group with the specified ID.
        /// </summary>
        /// <param name="key">ID that identifies the Unit Type Group to be retrieved.</param>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// The DTO for the indicated Unit Type Group.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/UnitTypeGroups(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/UnitTypeGroupsController.doc' path='UnitTypeGroupsController/Methods[@name="GetOne"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery(MaxExpansionDepth=Constants.DefaultMaxExpansionDepth)]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(UnitTypeGroupQueryDto))]
        public async Task<IHttpActionResult> Get([FromODataUri] Guid key, ODataQueryOptions<UnitTypeGroupQueryDto> queryOptions)
        {
            var results = await Facade.Get(key);
            return Query(results);
        }
    }
}