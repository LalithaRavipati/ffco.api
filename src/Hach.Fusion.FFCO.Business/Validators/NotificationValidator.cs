using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Core.Dtos;
using System;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="NotificationDto"/>s.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class NotificationValidator : FFValidator<NotificationDto, Guid>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="NotificationDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(NotificationDto dto)
        {
            IsNull(dto);

            IsType(dto, typeof(NotificationDto));

            if (FFErrors.Count > 0)
                return new FFValidationResponse
                {
                    FFErrors = FFErrors
                };

            Evaluate(x => x.Message, dto.Message)
                .Required();

            if (!dto.BroadcastAll)
                Evaluate(x => x.GroupName, dto.GroupName)
                    .Required();

            return new FFValidationResponse
            {
                FFErrors = FFErrors
            };
        }
    }
}
