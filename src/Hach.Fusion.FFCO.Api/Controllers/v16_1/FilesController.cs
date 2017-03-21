using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using Hach.Fusion.Core.Api.Controllers;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Enums;
using Swashbuckle.Swagger.Annotations;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Helpers;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Web API controller for retrieving blob storage files whose metadata is stored in DocumentDb.
    /// </summary>
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
        /// <param name="id">ID that uniquely identifies the blob storage file to be retrieved.</param>
        /// <returns>
        /// The specified file.
        /// </returns>
        /// <example>
        /// GET: ~/api/v16.1/Files/id
        /// </example>
        /// <include file='XmlDocumentation/ParametersController.doc' path='ParametersController/Methods[@name="GetOne"]/*'/>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<HttpResponseMessage> Get(Guid id)
        {
            var results = await _facade.Get(id);

            return results;
        }
    }
}