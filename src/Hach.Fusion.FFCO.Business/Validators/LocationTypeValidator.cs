using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Dtos;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="LocationCommandDto"/>s.
    /// </summary>
    public class LocationTypeValidator : FFValidator<LocationTypeCommandDto, Guid>
    {
        private const int NameMaximumLength = 100;

        private const int NameMinimumLength = 4;

        /// <summary>
        /// Validates the state of the specified <see cref="LocationTypeCommandDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(LocationTypeCommandDto dto)
        {
            IsNull(dto);

            if (FFErrors.Count > 0)
                return new FFValidationResponse
                {
                    FFErrors = FFErrors
                };

            Evaluate(l => l.I18NKeyName, dto.I18NKeyName)
                .Required()
                .MaxLength(NameMaximumLength)
                .MinLength(NameMinimumLength);         

            return new FFValidationResponse
            {
                FFErrors = FFErrors
            };
        }      
    }
}
