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
using Hach.Fusion.Core.Enums;

using Swashbuckle.Swagger.Annotations;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.FFCO.Business.Facades;
using Microsoft.Data.OData;
using System.Collections.Generic;
using Hach.Fusion.Data.Dtos;

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
    public class InAppMessagesController
        : ControllerWithCruModelsBase<InAppMessageBaseDto, InAppMessageBaseDto, InAppMessageQueryDto, Guid>
    {
        private readonly IInAppMessageFacade _facade;

        /// <summary>
        /// Default constructor for the <see cref="InAppMessagesController"/> class taking OData helper and repository facade arguments.
        /// </summary>
        /// <param name="oDataHelper">Helper that provides OData utilities to manage requests.</param>
        /// <param name="facade">Facade for the repository used to persist In-Application Message data.</param>
        public InAppMessagesController(IODataHelper oDataHelper, IInAppMessageFacade facade) 
            : base(oDataHelper)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            Facade = _facade = facade;
        }

        /// <summary>
        /// Retrieves the In-Application Message for the specified User.
        /// </summary>
        /// <param name="userId">UserId that identifies the User's In-Application Messages to be retrieved.</param>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// The DTO for the indicated User's In-Application Messages.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/Extensions.GetByUserId(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/InAppMessagesController.doc' path='InAppMessagesController/Methods[@name="GetByUserId"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery(MaxExpansionDepth=Constants.DefaultMaxExpansionDepth)]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(ICollection<InAppMessageQueryDto>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, typeof(SwaggerResponseUnauthorized))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ODataError))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, null, typeof(SwaggerResponseInternalServerError))]
        [ResponseType(typeof(InAppMessageQueryDto))]
        [HttpGet]
        public async Task<IHttpActionResult> GetByUserId([FromODataUri] Guid userId, ODataQueryOptions<InAppMessageQueryDto> queryOptions)
        {
            var results = await _facade.GetByUserId(userId, queryOptions);
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
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(CommandResult<InAppMessageBaseDto, Guid>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, typeof(SwaggerResponseUnauthorized))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ODataError))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, null, typeof(SwaggerResponseInternalServerError))]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [ResponseType(typeof(CommandResult<InAppMessageBaseDto, Guid>))]
        public async Task<IHttpActionResult> Patch([FromODataUri] Guid key, Delta<InAppMessageBaseDto> delta)
        {
            var result = await _facade.Update(key, delta);
            return Command(result);
        }
    }
}