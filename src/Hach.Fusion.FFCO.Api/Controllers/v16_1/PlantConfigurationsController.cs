using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Hach.Fusion.Core.Api.Handlers;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Helpers;
using Hach.Fusion.Core.Dtos;
using Swashbuckle.Swagger.Annotations;
using System.Net;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Controller that manages plant configuration file uploads.
    /// This endpoint uploads plant configuration files to the cloud. The file is immediately persisted
    /// in BLOB storage, where it is permanently archived. The data in the file is then queued for
    /// further asynchronous processing by a web job (FFCO.WebJobs.BlobProcessor).
    /// </summary>
    [Authorize]
    [FFExceptionHandling]
    public class PlantConfigurationsController : ApiController
    {
        private readonly IPlantConfigurationsFacade _facade;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="facade">Class that provides the functionality for this controller.</param>
        public PlantConfigurationsController(IPlantConfigurationsFacade facade)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            _facade = facade;
        }

        /// <summary>
        /// Accepts a single xls file that contains configuration plant data.
        /// </summary>
        /// <returns></returns>
        /// /// <example>
        /// POST: ~/api/v16.1/PlantConfigurationsController
        /// </example>
        /// <include file='XmlDocumentation/PlantConfigurationsController.doc' path='PlantConfigurationsController/Methods[@name="Post"]/*'/>
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> Upload()
        {
            var errors = new List<FFErrorCode>();

            // Parse Request Content
            var provider = new MultipartFormDataStreamProvider(Path.GetTempPath());
            var parts = await Request.Content.ReadAsMultipartAsync(provider).ConfigureAwait(false);
            
            // Get files
            var files = parts.FileData.Select(x => x.LocalFileName);
            var enumeratedFiles = files as IList<string> ?? files.ToList();

            if (!enumeratedFiles.Any())
                errors.Add(GeneralErrorCodes.FormDataFilesMissing());

            if (errors.Any())
                return Request.CreateApiResponse(NoDtoHelpers.CreateCommandResult(errors));

            var fileToUpload = enumeratedFiles.First();
            var originalFileName = parts.FileData[0].Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

            FileUploadMetadataDto fileUploadMetadata = new FileUploadMetadataDto();
            fileUploadMetadata.SavedFileName = fileToUpload;
            fileUploadMetadata.OriginalFileName = originalFileName;
            fileUploadMetadata.TransactionType = parts.FormData["uploadTransactionType"].Replace("\"", string.Empty);

            var result = await _facade.Upload(fileUploadMetadata);

            foreach (var file in enumeratedFiles)
                File.Delete(file);

            return Request.CreateApiResponse(result);
        }
    }
}
