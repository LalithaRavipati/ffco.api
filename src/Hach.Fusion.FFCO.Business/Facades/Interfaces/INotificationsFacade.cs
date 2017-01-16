using System.Threading.Tasks;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.FFCO.Core.Dtos;

namespace Hach.Fusion.FFCO.Business.Facades.Interfaces
{
    public interface INotificationsFacade
    {
        /// <summary>
        /// Uses information in the dto to send notifications.
        /// </summary>
        /// <param name="dto">Dto containing notification information.</param>
        /// <returns>Task that returns the command result.</returns>
        Task<CommandResultNoDto> SendNotification(NotificationDto dto);
    }
}