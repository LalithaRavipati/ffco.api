using System.Threading.Tasks;

namespace Hach.Fusion.FFCO.Business.Notifications
{
    /// <summary>
    /// Interface to be implemented by classes that send notifications.
    /// </summary>
    public interface INotificationSender
    {
        /// <summary>
        /// Sends a notification to all users (broadcast message).
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing an asynchronous operation.</returns>
        Task SendAll(string message);

        /// <summary>
        /// Sends a notification to users in a specified group.
        /// </summary>
        /// <param name="group">The name of the group.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing an asynchronous operation.</returns>
        Task SendGroup(string group, string message);
    }
}