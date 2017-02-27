using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Dtos;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="LimitTypeBaseDto"/>s.
    /// </summary>
    public class LimitTypeValidator : FFValidator<LimitTypeBaseDto, Guid>
    {
        private const int NameMaximumLength = 100;
        private const int NameMinimumLength = 4;

        /// <summary>
        /// Validates the state of the specified <see cref="LimitTypeBaseDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(LimitTypeBaseDto dto)
        {
            IsNull(dto);

            if (FFErrors.Count > 0)
                return new FFValidationResponse
                {
                    FFErrors = FFErrors
                };

            Evaluate(x => x.I18NKeyName, dto.I18NKeyName)
                .Required()
                .MaxLength(NameMaximumLength)
                .MinLength(NameMinimumLength);

            Evaluate(x => x.Polarity, dto.Polarity)
                .Required();

            Evaluate(x => x.Severity, dto.Severity)
                .Required();

            return new FFValidationResponse
            {
                FFErrors = FFErrors
            };
        }
    }
}
