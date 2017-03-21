using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Hach.Fusion.Core.Business.Results;

namespace Hach.Fusion.FFCO.Business.Facades.Interfaces
{
    /// <summary>
    /// Interface for classes retrieving a file.
    /// </summary>
    public interface IFileFacade
    {
        /// <summary>
        /// Retrieves the indicated file.
        /// </summary>
        /// <param name="id">
        /// ID that uniquely identifies the file to be retrieved.
        /// </param>
        /// <returns>The file indicated by the specified ID.</returns>
        Task<HttpResponseMessage> Get(Guid id);
    }
}
