using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Entities;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="InAppMessageCommandDto"/>s.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class InAppMessageValidator : FFValidator<InAppMessageCommandDto, Guid>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="InAppMessageCommandDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(InAppMessageCommandDto dto)
        {
            IsNull(dto);

            IsType(dto, typeof(InAppMessageCommandDto));

            if (FFErrors.Count > 0)
                return new FFValidationResponse
                {
                    FFErrors = FFErrors
                };

            return new FFValidationResponse
            {
                FFErrors = FFErrors
            };
        }      
    }
}
