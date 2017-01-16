using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Hach.Fusion.Core.Api.Handlers;
using Hach.Fusion.FFCO.Business.Facades;
using Hach.Fusion.FFCO.Core.Dtos;

namespace Hach.Fusion.FFCO.Api.Controllers.v16_1
{
    /// <summary>
    /// Api controller form managing notifications.
    /// </summary>
    [Authorize]
    [FFExceptionHandling]
    [EnableCors("*", "*", "*")]
    public class NotificationsController : ApiController
    {
        private readonly INotificationsFacade _facade;

        /// <summary>
        /// Constructor for <see cref="NotificationsController"/>.
        /// </summary>
        /// <param name="facade">Facade that contains the logic to send notifications.</param>
        public NotificationsController(INotificationsFacade facade)
        {
            if (facade == null)
                throw new ArgumentNullException(nameof(facade));

            _facade = facade;
        }

        /// <summary>
        /// Post action that sends notifications.
        /// </summary>
        /// <param name="dto">Dto containing notification information.</param>
        /// <returns>Task that returns the result of the action.</returns>
        [HttpPost]
        public async Task<IHttpActionResult> SendNotification([FromBody] NotificationDto dto)
        {
            var result = await _facade.SendNotification(dto);

            return Request.CreateApiResponse(result);
        }
    }
}