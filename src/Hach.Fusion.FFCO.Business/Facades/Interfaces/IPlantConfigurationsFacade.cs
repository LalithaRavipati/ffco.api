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
        /// <returns>A task that returns the result of the upload option.</returns>
        Task<CommandResultNoDto> Upload(FileUploadMetadataDto fileName);
    }
}
