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
        /// Creates an operation configuration file and saves it to blob storage. When
        /// the file is ready to be downloaded, a signalr notification is sent to the user who made the
        /// requst.
        /// </summary>
        /// <param name="tenantId">Identifies the tenant that the operation belongs to.</param>
        /// <param name="operationId">Identifies the operation to populate the configuration spreadsheet for or
        /// null to create a configuration file template with no operation information.</param>
        /// <param name="authenticationHeader">Authentication header for the request.</param>
        /// <returns>A task that returns the result of the request.</returns>
        Task<CommandResultNoDto> Get(Guid tenantId, Guid? operationId, string authenticationHeader);
    }
}
