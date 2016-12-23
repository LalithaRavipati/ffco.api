using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Hach.Fusion.Core.Azure.Blob;
using Hach.Fusion.Core.Azure.Queue;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Helpers;
using Newtonsoft.Json;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// The facade that provides the functionality for plant configuration operations.
    /// </summary>
    public class PlantConfigurationsFacade : IPlantConfigurationsFacade
    {
        private readonly DataContext _context;
        private readonly IBlobManager _blobManager;
        private readonly IQueueManager _queueManager;

        private readonly string _blobStorageConnectionString;
        private readonly string _blobStorageContainerName;
        private readonly string _queueStorageContainerName;

        /// <summary>
        /// Constructor for the <see cref="PlantConfigurationsFacade"/>.
        /// </summary>
        /// <param name="context">Database context containing dashboard type entities.</param>
        /// <param name="blobManager">Manager for Azure Blob Storage.</param>
        /// <param name="queueManager">Manager for Azure Queue Storage.</param>
        public PlantConfigurationsFacade(DataContext context, IBlobManager blobManager, IQueueManager queueManager)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (blobManager == null)
                throw new ArgumentNullException(nameof(blobManager));
            if (queueManager == null)
                throw new ArgumentNullException(nameof(queueManager));

            _blobStorageConnectionString = ConfigurationManager.ConnectionStrings["BlobProcessorStorageConnectionString"].ConnectionString;
            _blobStorageContainerName = ConfigurationManager.AppSettings["BlobProcessorBlobStorageContainerName"];
            _queueStorageContainerName = ConfigurationManager.AppSettings["BlobProcessorQueueStorageContainerName"];

            _context = context;
            _blobManager = blobManager;
            _queueManager = queueManager;
        }

        /// <summary>
        /// Accepts a single xls file that contains plant configuration.
        /// </summary>
        /// <param name="fileName">The full name of the file to upload.</param>
        /// <returns>A task that returns the result of the upload option.</returns>
        public async Task<CommandResultNoDto> Upload(string fileName)
        {
            var errors = new List<FFErrorCode>();

            // TODO: Get tenant from token

            // Store file in blob storage.
            var result = await _blobManager.StoreAsync(_blobStorageConnectionString, _blobStorageContainerName, fileName);

            // Add message to queue.
            await _queueManager.AddAsync(_blobStorageConnectionString, _queueStorageContainerName, JsonConvert.SerializeObject(result));

            return NoDtoHelpers.CreateCommandResult(errors);
        }
    }
}
