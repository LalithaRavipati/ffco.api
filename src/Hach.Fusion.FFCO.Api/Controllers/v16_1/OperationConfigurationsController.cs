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
    /// Controller that manages operation configuration file uploads.
    /// </summary>
    /// <remarks>
    /// This endpoint uploads operation configuration files to the cloud. The file is immediately persisted
    /// in BLOB storage, where it is permanently archived. The data in the file is then queued for
    /// further asynchronous processing by a web job (FFCO.WebJobs.BlobProcessor).
    /// </remarks>
    [Authorize]
    [FFExceptionHandling]
    public class OperationConfigurationsController : ApiController
    {
        private readonly IOperationConfigurationsFacade _facade;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="facade">Class that provides the functionality for this controller.</param>
        public OperationConfigurationsController(IOperationConfigurationsFacade facade)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            _facade = facade;
        }

        /// <summary>
        /// Accepts a single xls file that contains operation configuration data.
        /// </summary>
        /// <returns></returns>
        /// /// <example>
        /// POST: ~/api/v16.1/OperationConfigurations
        /// </example>
        /// <include file='XmlDocumentation/OperationConfigurationsController.doc' path='OperationConfigurationsController/Methods[@name="Post"]/*'/>
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> Post()
        {
            var errors = new List<FFErrorCode>();

            // Parse Request Content
            var provider = new MultipartFormDataStreamProvider(Path.GetTempPath());
            var parts = await Request.Content.ReadAsMultipartAsync(provider).ConfigureAwait(false);

            // Validate the request body
            if (string.IsNullOrEmpty(parts.FormData["uploadTransactionType"]))
                errors.Add(GeneralErrorCodes.FormDataFieldMissing("uploadTransactionType"));

            var tenantIdString = parts.FormData["tenantId"];
            if (string.IsNullOrEmpty(tenantIdString))
                errors.Add(GeneralErrorCodes.FormDataFieldMissing("tenantId"));

            var tenantIdGuid = Guid.Empty;
            if (!Guid.TryParse(tenantIdString, out tenantIdGuid) || tenantIdGuid == Guid.Empty)
                errors.Add(GeneralErrorCodes.FormDataFieldInvalid("tenantId"));

            // Get files
            var files = parts.FileData.Select(x => x.LocalFileName);
            var enumeratedFiles = files as IList<string> ?? files.ToList();

            if (!enumeratedFiles.Any())
                errors.Add(GeneralErrorCodes.FormDataFilesMissing());
            
            if (errors.Any())
                return Request.CreateApiResponse(NoDtoHelpers.CreateCommandResult(errors));

            var fileToUpload = enumeratedFiles.First();
            var originalFileName = parts.FileData[0].Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

            var fileUploadMetadata = new FileUploadMetadataDto
            {
                SavedFileName = fileToUpload,
                OriginalFileName = originalFileName,
                TransactionType = parts.FormData["uploadTransactionType"].Replace("\"", string.Empty)
            };

            var authHeader = Request.Headers.Authorization.ToString();
            var result = await _facade.Upload(fileUploadMetadata, authHeader, tenantIdGuid);

            foreach (var file in enumeratedFiles)
                File.Delete(file);

            return Request.CreateApiResponse(result);
        }

        /// <summary>
        /// Creates a configuration file that contains operation configuration data and save it to blob storage. When
        /// the file is ready to be downloaded, a signalr notification is sent to the user who made the
        /// request.
        /// </summary>
        /// <param name="tenantId">Identifies the tenant that the operation belongs to.</param>
        /// <param name="operationId">Identifies the operation to create the configuration for or null to
        /// create a configuration template without operation information.</param>
        /// <returns>Task that returns the request result.</returns>
        /// /// <example>
        /// GET: ~/api/v16.1/OperationConfigurations/tenantId=xxx?operationId=xxx
        /// </example>
        /// <include file='XmlDocumentation/OperationConfigurationsController.doc' path='OperationConfigurationsController/Methods[@name="GetConfig"]/*'/>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetConfig([FromUri] Guid? tenantId, [FromUri] Guid? operationId = null)
        {
            var errors = new List<FFErrorCode>();

            if (!tenantId.HasValue)
                errors.Add(ValidationErrorCode.PropertyRequired(nameof(tenantId)));

            if (errors.Any())
                return Request.CreateApiResponse(NoDtoHelpers.CreateCommandResult(errors));

            var authHeader = Request.Headers.Authorization.ToString();
            var result = await _facade.Get((Guid)tenantId, operationId, authHeader);

            return Request.CreateApiResponse(result);
        }
    }
}
