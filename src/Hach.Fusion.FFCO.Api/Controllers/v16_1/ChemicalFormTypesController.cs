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
using Hach.Fusion.FFCO.Core.Dtos;
using Swashbuckle.Swagger.Annotations;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Web API controller for retriving ChemicalFormTypes.
    /// </summary>
    [EnableCors("*", "*", "*")]
    public class ChemicalFormTypesController : ControllerWithCruModelsBase<ChemicalFormTypeQueryDto, ChemicalFormTypeQueryDto, ChemicalFormTypeQueryDto, Guid>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oDataHelper">Helper that provides OData utilities to manage requests.</param>
        /// <param name="facade">Facade for the repository used to persist data.</param>
        public ChemicalFormTypesController(IODataHelper oDataHelper,
            IFacadeWithCruModels<ChemicalFormTypeQueryDto, ChemicalFormTypeQueryDto, ChemicalFormTypeQueryDto, Guid> facade)
            : base(oDataHelper)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            Facade = facade;
        }
        /// <summary>
        /// Retrieves a queryable list of ChemicalFormTypes.
        /// </summary>
        /// <param name="queryOptions">OData query options that provide for sorting and filtering.</param>
        /// <returns>
        /// A list of DTOs that satisfy query option criteria.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/ChemicalFormTypes
        /// </example>
        /// <include file='XmlDocumentation/ChemicalFormTypesController.doc' path='ChemicalFormTypesController/Methods[@name="GetAll"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(ChemicalFormTypeQueryDto))]
        public async Task<IHttpActionResult> Get(ODataQueryOptions<ChemicalFormTypeQueryDto> queryOptions)
        {
            var results = await Facade.Get(queryOptions);
            return Query(results);
        }

        /// <summary>
        /// Retrieves the ChemicalFormType with the specified ID.
        /// </summary>
        /// <param name="key">ID that identifies the entity to be retrieved.</param>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// The DTO for the indicated entity.
        /// </returns>
        /// <example>
        /// GET: ~/odata/v16.1/ChemicalFormTypes(CDB928DA-365A-431E-A419-E9D6AF0C4FE5)
        /// </example>
        /// <include file='XmlDocumentation/ChemicalFormTypesController.doc' path='ChemicalFormTypesController/Methods[@name="GetOne"]/*'/>
        [FFSEAuthorize(PermissionAction.Read)]
        [EnableQuery]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [ResponseType(typeof(ChemicalFormTypeQueryDto))]
        public async Task<IHttpActionResult> Get([FromODataUri] Guid key, ODataQueryOptions<ChemicalFormTypeQueryDto> queryOptions)
        {
            var results = await Facade.Get(key);
            return Query(results);
        }
    }
}