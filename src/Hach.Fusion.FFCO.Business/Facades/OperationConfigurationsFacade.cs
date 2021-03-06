﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Dtos;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Database;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Helpers;
using Newtonsoft.Json;
using System.Threading;
using System.Web.Http;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.Data.Azure.DocumentDB;
using Hach.Fusion.Data.Azure.Blob;
using Hach.Fusion.Data.Azure.Queue;
using Hach.Fusion.Data.Constants;
using Hach.Fusion.Data.Database.Interfaces;
using Hach.Fusion.Data.Entities;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// The facade that provides the functionality for operation configuration operations.
    /// </summary>
    public class OperationConfigurationsFacade : IOperationConfigurationsFacade
    {
        private readonly IBlobManager _blobManager;
        private readonly IQueueManager _queueManager;
        private readonly IDocumentDbRepository<UploadTransaction> _documentDb;
        private readonly DataContext _context;

        private readonly string _blobStorageConnectionString;
        private readonly string _blobStorageContainerName;
        private readonly string _queueStorageContainerName;

        /// <summary>
        /// Constructor for the <see cref="OperationConfigurationsFacade"/>.
        /// </summary>
        /// <param name="context">Database context containing dashboard type entities.</param>
        /// <param name="blobManager">Manager for Azure Blob Storage.</param>
        /// <param name="queueManager">Manager for Azure Queue Storage.</param>
        /// <param name="documentDb">Azure DocumentDB repository</param>
        public OperationConfigurationsFacade(DataContext context, IBlobManager blobManager, IQueueManager queueManager,
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
        /// Accepts a single xls file that contains operation configuration.
        /// </summary>
        /// <param name="fileMetadata">Metadata associated with the file upload request.</param>
        /// <param name="authenticationHeader">Authentication header for the request.</param>
        /// <param name="requestTenantId">The selected Tenant Id from the request import the Operation Config to</param>
        /// <returns>A task that returns the result of the upload option.</returns>
        public async Task<CommandResultNoDto> Upload(FileUploadMetadataDto fileMetadata, string authenticationHeader, Guid requestTenantId)
        {
            var errors = new List<FFErrorCode>();
            var odataHelper = new Core.Api.OData.ODataHelper();

            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                errors.Add(GeneralErrorCodes.TokenInvalid("UserId"));

            if (errors.Count >0)
                return NoDtoHelpers.CreateCommandResult(errors);

            // ReSharper disable once AssignNullToNotNullAttribute
            var userIdGuid = Guid.Parse(userId);

            // Check that the Tenant Id in the request body is in the user's claim tenants
            var tenants = odataHelper.GetTenantIds(Thread.CurrentPrincipal) as List<Guid>;

            // Check user has no tenants in their claim or if the tenantid in the request body is not in the claim 
            if (tenants == null || tenants.All(x => x != requestTenantId))
                errors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("TenantId"));

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);


            // Store file in blob storage.
            var result = await _blobManager.StoreAsync(_blobStorageConnectionString, _blobStorageContainerName, fileMetadata.SavedFileName);

            // Add file metadata to documentDB to later be retrieved by request
            // An Id property is created by documentDB and in populated in the result object
            await _documentDb.CreateItemAsync(
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
                AuthenticationHeader = authenticationHeader,
                // TenantId should be checked by the blob processor that it matches the tenant in the Operation Config to be processed
                TenantId = requestTenantId  
            };

            var msg = JsonConvert.SerializeObject(queueMessage);
            // Add message to queue.
            await _queueManager.AddAsync(_blobStorageConnectionString, _queueStorageContainerName, msg);

            return NoDtoHelpers.CreateCommandResult(errors);
        }

        /// <summary>
        /// Creates an operation configuration file and saves it to blob storage. When
        /// the file is ready to be downloaded, a signalr notification is sent to the user who made the
        /// requst.
        /// </summary>
        /// <param name="tenantId">Identifies the tenant that the operation belongs to.</param>
        /// <param name="operationId">Identifies the operation to create the configuration for or
        /// null to create a configuration file template with no operation related data.</param>
        /// <param name="authenticationHeader">Authentication header for the request.</param>
        /// <returns>A task that returns the result of the request.</returns>
        public async Task<CommandResultNoDto> Get(Guid tenantId, Guid? operationId, string authenticationHeader)
        {
            var errors = new List<FFErrorCode>();

            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                errors.Add(GeneralErrorCodes.TokenInvalid("UserId"));

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);

            // ReSharper disable once AssignNullToNotNullAttribute
            var userIdGuid = Guid.Parse(userId);

            // Check that the user has access to the requested tenant.
            var odataHelper = new Core.Api.OData.ODataHelper();
            var tenants = odataHelper.GetTenantIds(Thread.CurrentPrincipal) as List<Guid>;
            if (tenants == null || tenants.All(x => x != tenantId))
                errors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("TenantId"));

            if (operationId != null)
            {
                var operation = await _context.Locations.FirstOrDefaultAsync(x => x.Id == operationId).ConfigureAwait(false);
                if (operation == null)
                    errors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("OperationId"));
                else if (operation.ProductOfferingTenantLocations.All(x => x.TenantId != tenantId))
                    errors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("TenantId"));
            }

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);

            var transactionType = operationId == null 
                ? UploadTransactionTypes.ExportOperationTemplate : UploadTransactionTypes.ExportOperationConfig;

            var queueMessage = new BlobQueueMessage
            {
                BlobTransactionType = transactionType,
                UserId = userIdGuid,
                TenantId = tenantId,
                OperationId = operationId,
                AuthenticationHeader = authenticationHeader
            };

            // Add message to queue.
            await _queueManager.AddAsync(_blobStorageConnectionString, _queueStorageContainerName, JsonConvert.SerializeObject(queueMessage));

            return NoDtoHelpers.CreateCommandResult(errors);
        }


        /// <summary>
        /// Deletes the requested operation if the operation has no measurements associated to it's locations.
        /// </summary>
        /// <param name="operationId">Guid identitifer of the operation to delete</param>
        /// <returns>Task that returns the request result.</returns>
        public async Task<CommandResultNoDto> Delete(Guid? operationId)
        {
            var errors = new List<FFErrorCode>();

            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (string.IsNullOrEmpty(userId))
                errors.Add(GeneralErrorCodes.TokenInvalid("UserId"));

            if (!operationId.HasValue || operationId == Guid.Empty)
                errors.Add(ValidationErrorCode.PropertyRequired("OperationId"));

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);

            var userIdGuid = Guid.Parse(userId);

            List<Location> locations = new List<Location>();
            List<LocationParameterLimit> locationParameterLimits = new List<LocationParameterLimit>();
            List<LocationParameter> locationParameters = new List<LocationParameter>();
            List<LocationLogEntry> locationLocationLogEntries= new List<LocationLogEntry>();

            // Check that the Location exists and if it's an operation
            if (!_context.Locations.Any(x => x.Id == operationId.Value && x.LocationType.LocationTypeGroup.Id == Data.Constants.LocationTypeGroups.Operation.Id))
                errors.Add(EntityErrorCode.EntityNotFound);
            else
            {
                // Check if user is in the proper tenant.
                if (!_context.Locations.Single(x => x.Id == operationId.Value)
                        .ProductOfferingTenantLocations.Any(x => x.Tenant.Users.Any(u => u.Id == userIdGuid)))
                {
                    errors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("TenantId"));
                }
                // Check that the operation has no measurements or notes and can be deleted

                // NOTE : This method will not scale beyond more than 4 or 5 levels max
                var operation = _context.Locations.Include("Locations").Single(x => x.Id == operationId.Value);
                locations.Add(operation);
                var systems = operation.Locations;
                locations.AddRange(systems);
                foreach (var system in systems)
                {
                    locations.AddRange(_context.Locations.Where(x => x.ParentId == system.Id).ToList());
                }

                var locationIds = locations.Select(l => l.Id);
                locationParameters =
                    _context.LocationParameters.Where(x => locationIds.Contains(x.LocationId)).ToList();

                var locationParamIds = locationParameters.Select(lp => lp.Id);
                locationParameterLimits =
                    _context.LocationParameterLimits.Where(
                        x => locationParamIds.Contains(x.LocationParameterId)).ToList();

                var hasLocationLocationLogEntries =
                    _context.LocationLogEntries.Any(x => locationIds.Contains(x.LocationId));

                var hasMeasurements = _context.Measurements.Any(x => locationParamIds.Contains(x.LocationParameterId));
                var hasParameterNotes =
                    _context.LocationParameterNotes.Any(
                        x => locationParamIds.Contains(x.LocationParameterId));

                var hasMeasurementTransactions =
                    _context.MeasurementTransactions.Any(x => locationIds.Contains(x.LocationId));

                if (hasParameterNotes || hasMeasurements || hasLocationLocationLogEntries || hasMeasurementTransactions)
                    errors.Add(EntityErrorCode.EntityCouldNotBeDeleted);
            }

            if (errors.Count > 0)
                return NoDtoHelpers.CreateCommandResult(errors);

            _context.LocationParameterLimits.RemoveRange(locationParameterLimits);
            _context.LocationParameters.RemoveRange(locationParameters);
            _context.Locations.RemoveRange(locations);
            _context.LocationLogEntries.RemoveRange(locationLocationLogEntries);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            var commandResult = NoDtoHelpers.CreateCommandResult(errors);
            // CreateCommandResult will either return BadRequest(400) or Ok(200)
            // Overriding the Status code to return a 204 on a successful delete
            commandResult.StatusCode = FacadeStatusCode.NoContent;
            return commandResult;
        }
    }
}
