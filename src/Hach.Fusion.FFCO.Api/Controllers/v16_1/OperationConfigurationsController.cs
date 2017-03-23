﻿using System;
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
        /// POST: ~/api/v16.1/OperationConfigurationsController
        /// </example>
        /// <include file='XmlDocumentation/OperationConfigurationsController.doc' path='OperationConfigurationsController/Methods[@name="Post"]/*'/>
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

            if (string.IsNullOrEmpty(parts.FormData["uploadTransactionType"]))
                errors.Add(GeneralErrorCodes.FormDataFieldMissing("uploadTransactionType"));

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
            var result = await _facade.Upload(fileUploadMetadata, authHeader);

            foreach (var file in enumeratedFiles)
                File.Delete(file);

            return Request.CreateApiResponse(result);
        }

        /// <summary>
        /// Creates an xlxs file that contains operation configuration data and save it to blob storage. When
        /// the file is ready to be downloaded, a signalr notification is sent to the user who made the
        /// requst.
        /// </summary>
        /// <param name="tenantId">Identifies the tenant that the operation belongs to.</param>
        /// <param name="operationId">Identifies the operation to download the configuration for.</param>
        /// <returns>Task that returns the request result.</returns>
        /// /// <example>
        /// GET: ~/api/v16.1/OperationConfigurationsController
        /// </example>
        /// <include file='XmlDocumentation/OperationConfigurationsController.doc' path='OperationConfigurationsController/Methods[@name="Get"]/*'/>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> Download([FromUri] Guid? tenantId, [FromUri] Guid? operationId)
        {
            var errors = new List<FFErrorCode>();

            if (!tenantId.HasValue)
                errors.Add(ValidationErrorCode.PropertyRequired(nameof(tenantId)));

            if (!operationId.HasValue)
                errors.Add(ValidationErrorCode.PropertyRequired(nameof(operationId)));

            if (errors.Any())
                return Request.CreateApiResponse(NoDtoHelpers.CreateCommandResult(errors));

            var authHeader = Request.Headers.Authorization.ToString();
            var result = await _facade.Download((Guid)tenantId, (Guid)operationId, authHeader);

            return Request.CreateApiResponse(result);
        }
    }
}