using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Azure.Blob;
using Hach.Fusion.Data.Azure.DocumentDB;
using Hach.Fusion.Data.Database.Interfaces;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Helpers;
using Newtonsoft.Json;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Retrieves an Azure blob storage file whose metadata is stored in DocumentDb.
    /// </summary>
    public class FileFacade : IFileFacade
    {
        private readonly IBlobManager _blobManager;
        private readonly IDocumentDbRepository<UploadTransaction> _documentDb;

        private readonly string _blobStorageConnectionString;
        private readonly string _blobStorageContainerName;

        /// <summary>
        /// Constructor for the <see cref="FileFacade"/>.
        /// </summary>
        /// <param name="blobManager">Manager for Azure Blob Storage.</param>
        /// <param name="documentDb">Azure DocumentDB repository</param>
        public FileFacade(IBlobManager blobManager, IDocumentDbRepository<UploadTransaction> documentDb)
        {
            if (blobManager == null)
                throw new ArgumentNullException(nameof(blobManager));
            if (documentDb == null)
                throw new ArgumentNullException(nameof(documentDb));

            _blobStorageConnectionString = ConfigurationManager.ConnectionStrings["BlobProcessorStorageConnectionString"].ConnectionString;
            _blobStorageContainerName = ConfigurationManager.AppSettings["BlobProcessorBlobStorageContainerName"];

            _blobManager = blobManager;
            _documentDb = documentDb;
        }

        /// <inheritdoc />
        public async Task<CommandResultNoDto> Get(Guid id)
        {
            var errors = new List<FFErrorCode>();

            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                errors.Add(GeneralErrorCodes.TokenInvalid("UserId"));

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);

            var userIdGuid = Guid.Parse(userId);

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);

            using (var stream = new MemoryStream())
            {
                _blobManager.DownloadBlob(stream, _blobStorageConnectionString, _blobStorageContainerName, "BodyPart_0738841b-b676-4abc-9f4f-0cbedf006abb");
            }

            var result = await _blobManager.DownloadBlobAsync(null, _blobStorageConnectionString, _blobStorageContainerName, "");

            return NoDtoHelpers.CreateCommandResult(errors);
        }
    }
}