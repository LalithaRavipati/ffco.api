using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Core.Extensions;
using Hach.Fusion.FFCO.Dtos;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates the <see cref="LocationLogEntryCommandDto"/>s.
    /// </summary>
    public class LocationLogEntryValidator : FFValidator<LocationLogEntryCommandDto, Guid>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="LocationLogEntryCommandDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(LocationLogEntryCommandDto dto)
        {
            IsNull(dto);

            if (FFErrors.Count > 0)
                return new FFValidationResponse
                {
                    FFErrors = FFErrors
                };

            Evaluate(l => l.LocationId, dto.LocationId)
                .Required();

            Evaluate(l => l.LogEntry, dto.LogEntry)
                .Required();

            return new FFValidationResponse
            {
                FFErrors = FFErrors
            };
        }      
    }
}
