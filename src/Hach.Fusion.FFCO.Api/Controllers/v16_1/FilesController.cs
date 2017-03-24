using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Swashbuckle.Swagger.Annotations;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Microsoft.OData.Core;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Web API controller for retrieving blob storage files whose metadata is stored in DocumentDb.
    /// </summary>
    [EnableCors("*", "*", "*")]
    [Authorize]
    public class FilesController : ApiController
    {
        /// <summary>
        /// Facade that retrieves files from Azure blob storage.
        /// </summary>
        private readonly IFileFacade _facade;

        /// <summary>
        /// Default constructor for the <see cref="FilesController"/> class taking a facade argument.
        /// </summary>
        /// <param name="facade">Facade that retrieves files from Azure blob storage.</param>
        public FilesController(IFileFacade facade)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            _facade = facade;
        }

        /// <summary>
        /// Retrieves the blob storage file with the specified ID.
        /// </summary>
        /// <param name="id">
        /// ID that identifies the blob storage file to be retrieved. This is a key for the metadata describing the file.
        /// </param>
        /// <returns>
        /// The specified file.
        /// </returns>
        /// <example>
        /// GET: ~/api/v16.1/Files/id
        /// </example>
        /// <include file='XmlDocumentation/FilesController.doc' path='FilesController/Methods[@name="GetOne"]/*'/>
        [SwaggerResponse(HttpStatusCode.OK, "File as an HttpResponseMessage.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, null, typeof(SwaggerResponseUnauthorized))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ODataError))]
        [SwaggerResponse(HttpStatusCode.NotFound, null, typeof(SwaggerResponseNotFound))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, null, typeof(SwaggerResponseInternalServerError))]
        public async Task<HttpResponseMessage> Get(Guid? id)
        {
            var results = await _facade.Get(id);

            return results;
        }
    }
}