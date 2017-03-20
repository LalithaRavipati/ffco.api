using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Dtos;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Database;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Helpers;
using Newtonsoft.Json;
using System.Threading;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Data.Azure.DocumentDB;
using Hach.Fusion.Data.Azure.Blob;
using Hach.Fusion.Data.Azure.Queue;
using Hach.Fusion.Data.Database.Interfaces;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// The facade that provides the functionality for plant configuration operations.
    /// </summary>
    public class PlantConfigurationsFacade : IPlantConfigurationsFacade
    {
        private readonly IBlobManager _blobManager;
        private readonly IQueueManager _queueManager;
        private readonly IDocumentDbRepository<UploadTransaction> _documentDb;
        private readonly DataContext _context;

        private readonly string _blobStorageConnectionString;
        private readonly string _blobStorageContainerName;
        private readonly string _queueStorageContainerName;

        /// <summary>
        /// Constructor for the <see cref="PlantConfigurationsFacade"/>.
        /// </summary>
        /// <param name="context">Database context containing dashboard type entities.</param>
        /// <param name="blobManager">Manager for Azure Blob Storage.</param>
        /// <param name="queueManager">Manager for Azure Queue Storage.</param>
        /// <param name="documentDb">Azure DocumentDB repository</param>
        public PlantConfigurationsFacade(DataContext context, IBlobManager blobManager, IQueueManager queueManager, 
            IDocumentDbRepository<UploadTransaction> documentDb)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (blobManager == null)
                throw new ArgumentNullException(nameof(blobManager));
            if (queueManager == null)
                throw new ArgumentNullException(nameof(queueManager));
            if (documentDb == null)
                throw new ArgumentNullException(nameof(documentDb));

            _blobStorageConnectionString = ConfigurationManager.ConnectionStrings["BlobProcessorStorageConnectionString"].ConnectionString;
            _blobStorageContainerName = ConfigurationManager.AppSettings["BlobProcessorBlobStorageContainerName"];
            _queueStorageContainerName = ConfigurationManager.AppSettings["BlobProcessorQueueStorageContainerName"];

            _context = context;
            _blobManager = blobManager;
            _queueManager = queueManager;
            _documentDb = documentDb;
        }

        /// <summary>
        /// Accepts a single xls file that contains plant configuration.
        /// </summary>
        /// <param name="fileMetadata">Metadata associated with the file upload request.</param>
        /// <param name="authenticationHeader">Authentication header for the request.</param>
        /// <returns>A task that returns the result of the upload option.</returns>
        public async Task<CommandResultNoDto> Upload(FileUploadMetadataDto fileMetadata, string authenticationHeader)
        {
            var errors = new List<FFErrorCode>();

            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                errors.Add(GeneralErrorCodes.TokenInvalid("UserId"));

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);

            var userIdGuid = Guid.Parse(userId);

            var odataHelper = new Fusion.Core.Api.OData.ODataHelper();
            var tenants = odataHelper.GetTenantIds(Thread.CurrentPrincipal) as List<Guid>;

            // Store file in blob storage.
            var result = await _blobManager.StoreAsync(_blobStorageConnectionString, _blobStorageContainerName, fileMetadata.SavedFileName);

            // An Id property is created by documentDB and in populated in the result object
            var docDbresult = await _documentDb.CreateItemAsync(
                new UploadTransaction
                {
                    OriginalFileName = fileMetadata.OriginalFileName,
                    TenantIds = tenants,
                    UploadTransactionType = fileMetadata.TransactionType,
                    UserId = userIdGuid,
                    UtcTimestamp = DateTime.UtcNow
                });

            var queueMessage = new BlobQueueMessage
            {
                BlobName = result.BlobName,
                BlobSize = result.BlobSize,
                BlobUrl = result.BlobUrl,
                BlobTransactionType = fileMetadata.TransactionType,
                UserId = userIdGuid,
                AuthenticationHeader = authenticationHeader
            };

            // Add message to queue.
            await _queueManager.AddAsync(_blobStorageConnectionString, _queueStorageContainerName, JsonConvert.SerializeObject(queueMessage));

            return NoDtoHelpers.CreateCommandResult(errors);
        }

        /// <summary>
        /// Creates an xlxs file that contains configuration plant data and save it to blob storage. When
        /// the file is ready to be downloaded, a signalr notification is sent to the user who made the
        /// requst.
        /// </summary>
        /// <param name="tenantId">Identifies the tenant that the plant belongs to.</param>
        /// <param name="plantId">Identifies the plant to download the configuration for.</param>
        /// <param name="authenticationHeader">Authentication header for the request.</param>
        /// <returns>A task that returns the result of the request.</returns>
        public async Task<CommandResultNoDto> Download(Guid tenantId, Guid plantId, string authenticationHeader)
        {
            const string transactionType = "PlantConfigExport";
            var errors = new List<FFErrorCode>();

            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                errors.Add(GeneralErrorCodes.TokenInvalid("UserId"));

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);

            var userIdGuid = Guid.Parse(userId);

            var plant = await _context.Locations.FirstOrDefaultAsync(x => x.Id == plantId).ConfigureAwait(false);
            if (plant == null)
            {
                errors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("plantId"));
            }
            else
            {
                var validTenant = plant.ProductOfferingTenantLocations.Any(x => x.TenantId == tenantId);
                if (!validTenant)
                    errors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("tenantId"));
            }

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);

            var queueMessage = new BlobQueueMessage
            {
                BlobTransactionType = transactionType,
                UserId = userIdGuid,
                TenantId = tenantId,
                OperationId = plantId,
                AuthenticationHeader = authenticationHeader
            };

            // Add message to queue.
            await _queueManager.AddAsync(_blobStorageConnectionString, _queueStorageContainerName, JsonConvert.SerializeObject(queueMessage));

            return NoDtoHelpers.CreateCommandResult(errors);
        }
    }
}
