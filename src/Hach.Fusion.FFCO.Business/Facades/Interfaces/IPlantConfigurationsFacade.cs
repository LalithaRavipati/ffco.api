﻿using System;
using System.Threading.Tasks;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Dtos;

namespace Hach.Fusion.FFCO.Business.Facades.Interfaces
{
    /// <summary>
    /// Interface for the PlantConfigurationFacade.
    /// </summary>
    public interface IPlantConfigurationsFacade
    {
        /// <summary>
        /// Accepts a single xls file that contains plant configuration.
        /// </summary>
        /// <param name="fileName">The full name of the file to upload.</param>
        /// <param name="authenticationHeader">Authentication header for the request.</param>
        /// <returns>A task that returns the result of the upload option.</returns>
        Task<CommandResultNoDto> Upload(FileUploadMetadataDto fileName, string authenticationHeader);

        /// <summary>
        /// Creates an xlxs file that contains configuration plant data and save it to blob storage. When
        /// the file is ready to be downloaded, a signalr notification is sent to the user who made the
        /// requst.
        /// </summary>
        /// <param name="tenantId">Identifies the tenant that the plant belongs to.</param>
        /// <param name="plantId">Identifies the plant to download the configuration for.</param>
        /// <param name="authenticationHeader">Authentication header for the request.</param>
        /// <returns>A task that returns the result of the request.</returns>
        Task<CommandResultNoDto> Download(Guid tenantId, Guid plantId, string authenticationHeader);
    }
}
