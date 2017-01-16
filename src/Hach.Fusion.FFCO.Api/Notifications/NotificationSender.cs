using System;
using System.Threading.Tasks;
using Hach.Fusion.FFCO.Business.Facades.Interfaces;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Hach.Fusion.FFCO.Api.Notifications
{
    /// <summary>
    /// Class that handles sending signalr messages.
    /// </summary>
    public class NotificationSender : INotificationSender
    {
        public NotificationSender()
        {
        }

        /// <summary>
        /// Sends a signalr message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="group"></param>
        /// <returns>A task representing an asynchronous operation.</returns>
        private static Task SendMessage(string message, string group = "")
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();

            return string.IsNullOrEmpty(group)
                ? hub.Clients.All.message(message)
                : hub.Clients.Group(group.ToUpper()).message(message);
        }

        /// <summary>
        /// Sends a notification to all users (broadcast message).
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing an asynchronous operation.</returns>
        public Task SendAll(string message)
        {
            return SendMessage(message);
        }

        /// <summary>
        /// Sends a notification to users in a specified group.
        /// </summary>
        /// <param name="group">The name of the group.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing an asynchronous operation.</returns>
        public Task SendGroup(string group, string message)
        {
            if (string.IsNullOrEmpty(group))
                throw new ArgumentNullException(nameof(group));

            return SendMessage(message, group.ToUpper());
        }


        /*private const string DefaultSignalRBaseUrl = "http://localhost"; // "http://localhost/signalr";
        private const string DefaultSignalRHubName = "notifications";
        private const string DefaultSendBroadcastMessageMethodName = "sendBroadcastMessage";
        private const string DefaultSendGroupMessageMethodName = "sendGroupMessage";

        private static string _signalrBaseUrl;
        private static string _signalRHubName;
        private static string _sendBroadcastMessageMethodName;
        private static string _sendGroupMessageMethodName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="signalRBaseUrl">The base url of the api hosting signalr. This api is contained in
        /// the Hach.Fusion.FFCO.Api project.</param>
        /// <param name="signalRHubName">The name of the signalr hub for sending in-app messages.</param>
        /// <param name="sendBroadcastMessageMethodName">The name of the hub method for sending broadcast messages.</param>
        /// <param name="sendGroupMessageMethodName">The name of the hum method for sending group messages.</param>
        public NotificationSender(string signalRBaseUrl = DefaultSignalRBaseUrl,
            string signalRHubName = DefaultSignalRHubName,
            string sendBroadcastMessageMethodName = DefaultSendBroadcastMessageMethodName,
            string sendGroupMessageMethodName = DefaultSendGroupMessageMethodName)
        {
            _signalrBaseUrl = signalRBaseUrl;
            _signalRHubName = signalRHubName;
            _sendBroadcastMessageMethodName = sendBroadcastMessageMethodName;
            _sendGroupMessageMethodName = sendGroupMessageMethodName;
        }

        /// <summary>
        /// Sends a signalr message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="group"></param>
        /// <returns>A task representing an asynchronous operation.</returns>
        private static Task SendMessage(string message, string group = "")
        {
            // Make a connection to the signalR "inAppNotificationsHub" in the "Hach.Fusion.FFNE.Api" project.
            var connection = new HubConnection($"{_signalrBaseUrl}");
            var hubProxy = connection.CreateHubProxy(_signalRHubName);
            //connection.Start().Wait();


            try
            {
                connection.Start().Wait();
            }
            catch (Exception ex)
            {
                throw;
            }
            

            return string.IsNullOrEmpty(group)
                ? hubProxy.Invoke(_sendBroadcastMessageMethodName, message)
                : hubProxy.Invoke(_sendGroupMessageMethodName, group, message);
        }

        /// <summary>
        /// Sends a notification to all users (broadcast message).
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing an asynchronous operation.</returns>
        public Task SendAll(string message)
        {
            return SendMessage(message);
        }

        /// <summary>
        /// Sends a notification to users in a specified group.
        /// </summary>
        /// <param name="group">The name of the group.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>A task representing an asynchronous operation.</returns>
        public Task SendGroup(string group, string message)
        {
            if (string.IsNullOrEmpty(group))
                throw new ArgumentNullException(nameof(group));

            return SendMessage(message, group.ToUpper());
        }*/
    }
}
