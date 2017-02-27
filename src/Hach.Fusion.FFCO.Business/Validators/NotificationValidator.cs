using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Dtos;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="NotificationBaseDto"/>s.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class NotificationValidator : FFValidator<NotificationBaseDto>
    {
        /// <summary>
        /// Validates the state of the specified <see cref="NotificationBaseDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(NotificationBaseDto dto)
        {
            IsNull(dto);

            IsType(dto, typeof(NotificationQueryDto));

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
