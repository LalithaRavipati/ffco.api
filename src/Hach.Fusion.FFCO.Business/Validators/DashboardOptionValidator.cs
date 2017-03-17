using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Dtos;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="DashboardOptionBaseDto"/>s.
    /// </summary>
    public class DashboardOptionValidator : FFValidator<DashboardOptionBaseDto, Guid>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="DashboardOptionBaseDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(DashboardOptionBaseDto dto)
        {
            IsNull(dto);

            if (FFErrors.Count > 0)
                return new FFValidationResponse
                {
                    FFErrors = FFErrors
                };

            Evaluate(x => x.TenantId, dto.TenantId)
                .Required();

            return new FFValidationResponse
            {
                FFErrors = FFErrors
            };
        }
    }
}
