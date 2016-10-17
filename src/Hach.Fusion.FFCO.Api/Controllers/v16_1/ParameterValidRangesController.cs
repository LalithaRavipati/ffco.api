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
    /// Web API controller for managing Parameter Valid Ranges.
    /// </summary>
    /// <remarks>
    /// All of the public methods return an asynchronous task result containing information needed to create
    /// an API response message. Client applications using this API will not receive the task, but will instead receive
    /// a response message that originates from information in the task result. Since clients using this API, will
    /// see the XML comments in this class, the "return" fields below indicate the information returned to the
    /// client applications.
    /// </remarks>
    [EnableCors("*", "*", "*")]
    public class ParameterValidRangesController : FFAABaseController<ParameterValidRangeQueryDto, Guid>
    {
        /// <summary>
        /// Default constructor for the <see cref="ParameterValidRangesController"/> class taking OData helper and repository facade arguments.
        /// </summary>
        /// <param name="oDataHelper">Helper that provides OData utilities to manage requests.</param>
        /// <param name="facade">Facade for the repository used to retrieve location type data.</param>
        public ParameterValidRangesController(IODataHelper oDataHelper, IFacade<ParameterValidRangeQueryDto, Guid> facade) 
            : base(oDataHelper)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            _facade = facade;
        }

        /// <summary>
        /// Retrieves a queryable list of Parameter Valid Ranges.
        /// </summary>
        /// <param name="queryOptions">OData query options that provide for things like sorting and filtering.</param>
        /// <returns>
        /// A list of DTOs for the Parameter Valid Ranges that satisfy query option criteria.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/ParameterValidRanges
        /// </example>
        /// <include file='XmlDocumentation/ParameterValidRangesController.doc' path='ParameterValidRangesController/Methods[@name="GetAll"]/*'/>
        [FFSEAuthorize(PermissionAction.Read, PermissionResource.LocationLogEntry)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(ParameterValidRangeQueryDto))]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<ParameterValidRangeQueryDto> queryOptions)
        {
            var results = await _facade.Get(queryOptions);
            return Query(results);
        }

        /// <summary>
        /// Retrieves the Parameter Valid Range with the specified ID.
        /// </summary>
        /// <param name="key">ID that identifies the entity to be retrieved.</param>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// The DTO for the indicated Parameter.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/ParameterValidRanges(29522337-0153-48C7-9796-E16779199138)
        /// </example>
        /// <include file='XmlDocumentation/ParameterValidRangesController.doc' path='ParameterValidRangesController/Methods[@name="GetOne"]/*'/>
        [FFSEAuthorize(PermissionAction.Read, PermissionResource.LocationLogEntry)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(ParameterValidRangeQueryDto))]
        public async Task<IHttpActionResult> Get([FromODataUri] Guid key, ODataQueryOptions<ParameterDto> queryOptions)
        {
            var results = await _facade.Get(key);
            return Query(results);
        }
    }
}