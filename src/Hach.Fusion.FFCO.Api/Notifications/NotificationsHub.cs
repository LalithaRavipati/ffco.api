using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Hach.Fusion.FFCO.Api.Notifications
{
    /// <summary>
    /// SignalR hub supporting notifications.
    /// </summary>
    [HubName("notifications")]
    public class NotificationsHub : Hub
    {
        /// <summary>
        /// Adds a user to a group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>Task representing an asynchronous operation.</returns>
        public Task AddToGroup(string groupName)
        {
            return Groups.Add(Context.ConnectionId, groupName.ToUpper());
        }

        /// <summary>
        /// Sends a message to all connected users.
        /// </summary>
        /// <param name="message">Message to send.</param>
        public void SendBroadcastMessage(string message)
        {
            Clients.All.message(message);
        }

        /// <summary>
        /// Sends a message to all connected users that belong to the specified group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="message">Message to send.</param>
        public void SendGroupMessage(string groupName, string message)
        {
            Clients.Group(groupName.ToUpper()).message(message);
        }
    }
}