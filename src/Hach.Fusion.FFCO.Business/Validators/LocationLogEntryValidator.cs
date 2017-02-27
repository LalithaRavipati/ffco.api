using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Dtos;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates the <see cref="LocationLogEntryBaseDto"/>s.
    /// </summary>
    public class LocationLogEntryValidator : FFValidator<LocationLogEntryBaseDto, Guid>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="LocationLogEntryBaseDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(LocationLogEntryBaseDto dto)
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

            Evaluate(l => l.TimeStamp, dto.TimeStamp)
                .Required();

            return new FFValidationResponse
            {
                FFErrors = FFErrors
            };
        }      
    }
}
