using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Dtos;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="InAppMessageBaseDto"/>s.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class InAppMessageValidator : FFValidator<InAppMessageBaseDto, Guid>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="InAppMessageBaseDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(InAppMessageBaseDto dto)
        {
            IsNull(dto);

            IsType(dto, typeof(InAppMessageBaseDto));

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
