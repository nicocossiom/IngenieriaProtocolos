using System.Net;
using System.Net.Sockets;

namespace UdpChat.Lib
{
    /// <summary>
    /// An abstract class that represents a sendable object in the chat system. For example a <see cref="Request"/> or a <see cref="Response"/>.
    /// </summary>
    public class ChatSendable
    {
        /// <summary>
        /// Gets or sets the timestamp of the request.
        /// </summary>
        /// <value>The timestamp of the request.</value>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        /// <summary>
        /// Serializes the object and sends it to the specified endpoint using the specified client.
        /// </summary>
        /// <param name="endpoint">The endpoint to send this ChatSendable to</param>
        /// <param name="sender">The UdpClient sending this ChatSendable</param>
        /// <returns>The numbers of bytes sent</returns>
        public int SerializeAndSend(ref IPEndPoint endpoint, ref UdpClient sender)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return sender.Send(bytes, bytes.Length, endpoint);
        }
    }
}