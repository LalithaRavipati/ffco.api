using System;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Dtos.Dashboards;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="DashboardCommandDto"/>s.
    /// </summary>
    public class DashboardValidator : FFValidator<DashboardCommandDto, Guid>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="DashboardCommandDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(DashboardCommandDto dto)
        {
            IsNull(dto);

            if (FFErrors.Count > 0)
                return new FFValidationResponse
                {
                    FFErrors = FFErrors
                };

            Evaluate(x => x.OwnerUserId, dto.OwnerUserId)
                .Required();

            Evaluate(x => x.TenantId, dto.TenantId)
                .Required();

            Evaluate(x => x.DashboardOptionId, dto.DashboardOptionId)
                .Required();

            return new FFValidationResponse
            {
                FFErrors = FFErrors
            };
        }
    }
}
