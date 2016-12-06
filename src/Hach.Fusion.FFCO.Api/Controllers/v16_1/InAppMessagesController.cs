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
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Core.Dtos;
using Swashbuckle.Swagger.Annotations;
using Hach.Fusion.Core.Business.Results;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Web API controller for managing In-Application Messages.
    /// </summary>
    /// <remarks>
    /// All of the public methods below return an asynchronous task result containing information needed to create
    /// an API response message. Client applications using this API will not receive the task, but will instead receive
    /// a response message that originates from information in the task result. Since clients using this API, will
    /// see the XML comments in this class, the "return" fields below indicate the information returned to the
    /// client applications.
    /// </remarks>
    [EnableCors("*", "*", "*")]
    public class InAppMessagesController
        : ControllerWithCruModelsBase<InAppMessageCommandDto, InAppMessageCommandDto, InAppMessageQueryDto, Guid>
    {
        /// <summary>
        /// Default constructor for the <see cref="InAppMessagesController"/> class taking OData helper and repository facade arguments.
        /// </summary>
        /// <param name="oDataHelper">Helper that provides OData utilities to manage requests.</param>
        /// <param name="facade">Facade for the repository used to persist In-Application Message data.</param>
        public InAppMessagesController(IODataHelper oDataHelper, IFacadeWithCruModels<InAppMessageCommandDto, InAppMessageCommandDto, InAppMessageQueryDto, Guid> facade) 
            : base(oDataHelper)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            Facade = facade;
        }

        /// <summary>
        /// Retrieves a queryable list of In-Application Messages.
        /// </summary>
        /// <param name="queryOptions">OData query options that provide for sorting and filtering.</param>
        /// <returns>
        /// A list of DTOs for the In-Application Messages that satisfy query option criteria.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/InAppMessages
        /// </example>
        /// <include file='XmlDocumentation/InAppMessagesController.doc' path='InAppMessagesController/Methods[@name="GetAll"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(InAppMessageQueryDto))]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<InAppMessageQueryDto> queryOptions)
        {
            var results = await Facade.Get(queryOptions);
            return Query(results);
        }

        // TODO - akrone - Change the Get methods to a single Custom Action named "Extensions.GetByUserId()"

        /// <summary>
        /// Retrieves the In-Application Message with the specified ID.
        /// </summary>
        /// <param name="key">ID that identifies the In-Application Message to be retrieved.</param>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// The DTO for the indicated In-Application Message.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/InAppMessages(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/InAppMessagesController.doc' path='InAppMessagesController/Methods[@name="GetOne"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(InAppMessageQueryDto))]
        public async Task<IHttpActionResult> Get([FromODataUri] Guid key, ODataQueryOptions<InAppMessageQueryDto> queryOptions)
        {
            var results = await Facade.Get(key);
            return Query(results);
        }


        /// <summary>
        /// Replaces the specified properties of the indicated In-Application Message.
        /// </summary>
        /// <param name="key">Key that uniquely identifies the entity to be edited.</param>
        /// <param name="delta">Delta for the updated entity properties.</param>
        /// <returns>
        /// If successful, this method always returns "No Content".
        /// </returns>
        /// <example>
        /// PATCH: ~/odata/v16.1/InAppMessages(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/InAppMessagesController.doc' path='InAppMessagesController/Methods[@name="Patch"]/*'/>
        [FFSEAuthorize(PermissionAction.Update)]
        [AcceptVerbs("PATCH")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(CommandResult<InAppMessageCommandDto, Guid>))]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(CommandResult<InAppMessageCommandDto, Guid>))]
        public async Task<IHttpActionResult> Patch([FromODataUri] Guid key, Delta<InAppMessageCommandDto> delta)
        {
            var result = await Facade.Update(key, delta);
            return Command(result);
        }
    }
}