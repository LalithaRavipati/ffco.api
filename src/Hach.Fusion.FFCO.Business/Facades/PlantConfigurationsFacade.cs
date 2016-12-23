using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Hach.Fusion.Core.Blob;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Helpers;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// The facade that provides the functionality for plant configuration operations.
    /// </summary>
    public class PlantConfigurationsFacade : IPlantConfigurationsFacade
    {
        private readonly DataContext _context;
        private readonly IBlobManager _blobManager;

        /// <summary>
        /// Constructor for the <see cref="PlantConfigurationsFacade"/>.
        /// </summary>
        /// <param name="context">Database context containing dashboard type entities.</param>
        /// <param name="blobManager">Manager for Azure Blob Storage.</param>
        public PlantConfigurationsFacade(DataContext context, IBlobManager blobManager)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (blobManager == null)
                throw new ArgumentNullException(nameof(blobManager));

            _context = context;
            _blobManager = blobManager;
        }

        /// <summary>
        /// Accepts a single xls file that contains plant configuration.
        /// </summary>
        /// <param name="fileName">The full name of the file to upload.</param>
        /// <returns>A task that returns the result of the upload option.</returns>
        public async Task<CommandResultNoDto> Upload(string fileName)
        {
            var blobStorageConnectionString = ConfigurationManager.AppSettings["BlobProcessorStorageConnectionString"];
            var blobStorageContainerName = ConfigurationManager.AppSettings["BlobProcessorStorageContainerName"];

            var errors = new List<FFErrorCode>();

            // TODO: Get tenant from token

            // Store file in blob storage.
            var result = await _blobManager.StoreAsync(blobStorageConnectionString, blobStorageContainerName, fileName);

            // TODO: Add to queue

            return NoDtoHelpers.CreateCommandResult(errors);
        }
    }
}
