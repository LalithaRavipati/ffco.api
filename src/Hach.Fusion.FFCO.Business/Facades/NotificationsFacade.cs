using System;
using System.Threading.Tasks;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Core.Enums;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Hach.Fusion.FFCO.Business.Notifications;
using Hach.Fusion.FFCO.Core.Dtos;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// The class that performs the logic for the Notifications api.
    /// </summary>
    public class NotificationsFacade : INotificationsFacade
    {
        private readonly INotificationSender _notificationSender;
        private readonly IFFValidator<NotificationDto> _validator;

        /// <summary>
        /// Constructor for <see cref="NotificationsFacade"/>.
        /// </summary>
        /// <param name="notificationSender">Object responsible for sending notifications.</param>
        /// <param name="validator">Validates <see cref="NotificationDto"/>.</param>
        public NotificationsFacade(INotificationSender notificationSender, IFFValidator<NotificationDto> validator)
        {
            if (notificationSender == null)
                throw new ArgumentNullException(nameof(notificationSender));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            _notificationSender = notificationSender;
            _validator = validator;
        }

        /// <summary>
        /// Uses information in the dto to send notifications.
        /// </summary>
        /// <param name="dto">Dto containing notification information.</param>
        /// <returns>Task that returns the command result.</returns>
        public async Task<CommandResultNoDto> SendNotification(NotificationDto dto)
        {
            var validationResult = _validator.Validate(dto);

            if (validationResult.IsInvalid)
                return new CommandResultNoDto(validationResult);

            if (dto.BroadcastAll)
                await _notificationSender.SendAll(dto.Message);
            else
                await _notificationSender.SendGroup(dto.GroupName, dto.Message);

            return new CommandResultNoDto(FacadeStatusCode.Ok);
        }
    }
}
