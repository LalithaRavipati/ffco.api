using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Core.Dtos;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="NotificationDto"/>s.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class NotificationValidator : FFValidator<NotificationDto>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="NotificationDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(NotificationDto dto)
        {
            IsNull(dto);

            IsType(dto, typeof(InAppMessageCommandDto));

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
