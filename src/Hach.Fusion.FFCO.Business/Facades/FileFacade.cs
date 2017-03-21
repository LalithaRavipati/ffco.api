using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Hach.Fusion.Core.Api.Security;using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Azure.Blob;
using Hach.Fusion.Data.Azure.DocumentDB;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Database.Interfaces;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Retrieves an Azure blob storage file whose metadata is stored in DocumentDb.
    /// </summary>
    public class FileFacade : IFileFacade
    {
        private readonly DataContext _context;
        private readonly IDocumentDbRepository<UploadTransaction> _documentDb;
        private readonly IBlobManager _blobManager;

        private readonly string _blobStorageConnectionString;

        /// <summary>
        /// Constructor for the <see cref="FileFacade"/> class taking datbase context, DocumentDb repository,
        /// and blob manager arguments.
        /// </summary>
        /// <param name="context">Database context used to access user and tenant information.</param>
        /// <param name="documentDb">Azure DocumentDB repository containing file metadata.</param>
        /// <param name="blobManager">Manager used to access Azure Blob Storage.</param>
        public FileFacade(DataContext context, IDocumentDbRepository<UploadTransaction> documentDb, IBlobManager blobManager)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (documentDb == null)
                throw new ArgumentNullException(nameof(documentDb));
            if (blobManager == null)
                throw new ArgumentNullException(nameof(blobManager));

            _context = context;
            _blobManager = blobManager;
            _documentDb = documentDb;

            _blobStorageConnectionString = ConfigurationManager.ConnectionStrings["BlobProcessorStorageConnectionString"].ConnectionString;
        }

        /// <summary>
        /// Retrieves the indicated file.
        /// </summary>
        /// <param name="id">
        /// ID that uniquely identifies the file to be retrieved. The ID is is the key of the
        /// metadata stored in DocumentDb.
        /// </param>
        /// <returns>The file indicated by the specified ID.</returns>
        public async Task<HttpResponseMessage> Get(Guid id)
        {
            // Check that token contains a user ID
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return HandleErrors(GeneralErrorCodes.TokenInvalid("UserId"), HttpStatusCode.Unauthorized);

            // Make sure user ID is active
            var userIdGuid = new Guid(userId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid);
            if (user == null || !user.IsActive)
                return HandleErrors(GeneralErrorCodes.TokenInvalid("UserId"), HttpStatusCode.Unauthorized);

            // Make sure there's a tenant associated with the user
            if (user.Tenants.Count < 1)
                return HandleErrors(GeneralErrorCodes.TokenInvalid("UserId"), HttpStatusCode.Unauthorized);

            // Retrieve the metadata associated with the specified ID
            var metadata = await _documentDb.GetItemAsync(id.ToString());
            if (metadata == null || string.IsNullOrWhiteSpace(metadata.BlobStorageContainerName) ||
                string.IsNullOrWhiteSpace(metadata.BlobStorageBlobName))
                return HandleErrors(EntityErrorCode.EntityNotFound, HttpStatusCode.NotFound);

            // Make sure the user is authorized (intentionally returns Not Found even though it's an authorization problem)
            if (metadata.TenantIds.Count < 1 ||
                !metadata.TenantIds.Intersect(user.Tenants.Select(u => u.Id)).Any())
                return HandleErrors(EntityErrorCode.EntityNotFound, HttpStatusCode.NotFound);

            // Retrieve the blob
            var stream = new MemoryStream();
            var result =
                await _blobManager.DownloadBlobAsync(stream, _blobStorageConnectionString,
                      metadata.BlobStorageContainerName, metadata.BlobStorageBlobName);
            if (stream.Length == 0)
                return HandleErrors(EntityErrorCode.EntityNotFound, HttpStatusCode.NotFound);

            // Create the response
            stream.Position = 0;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };

            // Determine file name
            string filename = result.BlobName;
            if (!string.IsNullOrWhiteSpace(metadata.OriginalFileName))
            {
                var fileInfo = new FileInfo(metadata.OriginalFileName);
                if (!string.IsNullOrWhiteSpace(fileInfo.Name))
                {
                    filename = fileInfo.Name;
                }    
            }

            // Set response content headers
            response.Content.Headers.ContentLength = stream.Length;
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = filename,
                Size = stream.Length
            };

            return response;
        }

        /// <summary>
        /// Returns an HTTP response message for reporting an error to the client.
        /// </summary>
        /// <param name="errorCode">Fusion Foundation error object.</param>
        /// <param name="statusCode">HTTP status code.</param>
        /// <returns></returns>
        private HttpResponseMessage HandleErrors(FFErrorCode errorCode, HttpStatusCode statusCode)
        {
            var errors = new [] {new  {errorCode = errorCode.Code, message = errorCode.Description}}.ToList();

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new ObjectContent(errors.GetType(), errors, new JsonMediaTypeFormatter())
            };

            return response;
        }
    }
}