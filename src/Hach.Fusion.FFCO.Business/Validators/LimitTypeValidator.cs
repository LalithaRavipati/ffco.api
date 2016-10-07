﻿using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Dtos.LimitTypes;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="LimitTypeCommandDto"/>s.
    /// </summary>
    public class LimitTypeValidator : FFValidator<LimitTypeCommandDto, Guid>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="LimitTypeCommandDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(LimitTypeCommandDto dto)
        {
            IsNull(dto);

            if (FFErrors.Count > 0)
                return new FFValidationResponse
                {
                    FFErrors = FFErrors
                };

            Evaluate(x => x.I18NKeyName, dto.I18NKeyName)
                .Required();

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
