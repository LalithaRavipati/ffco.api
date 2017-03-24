using System;
using System.Threading.Tasks;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Business.Facades.Interfaces
{
    /// <summary>
    /// Interface for classes that provide Operation Configuration CRUD functionality.
    /// </summary>
    public interface IOperationConfigurationsFacade
    {
        /// <summary>
        /// Accepts a single xls file that contains operation configuration.
        /// </summary>
        /// <param name="fileName">The full name of the file to upload.</param>
        /// <param name="authenticationHeader">Authentication header for the request.</param>
        /// <returns>A task that returns the result of the upload option.</returns>
        Task<CommandResultNoDto> Upload(FileUploadMetadataDto fileName, string authenticationHeader);

        /// <summary>
        /// Creates an xlxs file that contains operation configuration data and save it to blob storage. When
        /// the file is ready to be downloaded, a signalr notification is sent to the user who made the
        /// requst.
        /// </summary>
        /// <param name="tenantId">Identifies the tenant that the operation belongs to.</param>
        /// <param name="operationId">Identifies the operation to download the configuration for.</param>
        /// <param name="authenticationHeader">Authentication header for the request.</param>
        /// <returns>A task that returns the result of the request.</returns>
        Task<CommandResultNoDto> Download(Guid tenantId, Guid operationId, string authenticationHeader);

        /// <summary>
        /// Deletes the requested operation if the operation has no measurements associated to it's locations.
        /// </summary>
        /// <param name="operationId">Identifies the operation to delete</param>
        /// <returns>Task that returns the request result.</returns>
        Task<CommandResultNoDto> Delete(Guid? operationId);
    }
}
