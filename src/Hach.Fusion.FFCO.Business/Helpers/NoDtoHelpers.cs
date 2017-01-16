using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.Core.Extensions;

namespace Hach.Fusion.FFCO.Business.Helpers
{
    /// <summary>
    /// Helper methods for Controllers and Facades that don't inherit from the
    /// base classes in Core.
    /// </summary>
    public static class NoDtoHelpers
    {
        /// <summary>
        /// Creates a response based on list of errors.
        /// </summary>
        /// <param name="errors">Class containing facade results.</param>
        /// <returns>A response that can be return by a controller method.</returns>
        public static CommandResultNoDto CreateCommandResult(List<FFErrorCode> errors)
        {
            return new CommandResultNoDto
            {
                ErrorCodes = errors,
                StatusCode = errors.Any() ? FacadeStatusCode.BadRequest : FacadeStatusCode.Ok
            };
        }

        /// <summary>
        /// Creates a response based on facade results.
        /// </summary>
        /// <param name="request">The current http request.</param>
        /// <param name="facadeResult">Class containing facade results.</param>
        /// <returns>A response that can be return by a controller method.</returns>
        public static IHttpActionResult CreateApiResponse(this HttpRequestMessage request, FacadeResult facadeResult)
        {
            var errors = facadeResult.ErrorCodes?
                .Select(error => new
                {
                    ErrorCode = error.Code,
                    Message = error.Description
                }).ToList();

            var response = request.CreateResponse((HttpStatusCode)facadeResult.StatusCode);
            response.Content = new StringContent(errors.ToJsonFormatted());

            return new ResponseMessageResult(response);
        }
    }
}
