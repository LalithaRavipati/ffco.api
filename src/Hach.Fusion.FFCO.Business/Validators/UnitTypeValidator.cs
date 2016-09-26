using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Dtos;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="LocationCommandDto"/>s.
    /// </summary>
    /// <remarks>
    /// In addition to the override of the <see cref="Validate"/> method, this class contains a static method
    /// used to test whether updating the parent of a location would result in a circular reference.
    /// </remarks>
    public class UnitTypeValidator : FFValidator<UnitTypeQueryDto, Guid>
    {
        private const int NameMaximumLength = 100;

        private const int NameMinimumLength = 4;

        /// <summary>
        /// Validates the state of the specified <see cref="LocationQueryDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(UnitTypeQueryDto dto)
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

            Evaluate(l => l.I18NKeyAbbreviation, dto.I18NKeyAbbreviation)
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
