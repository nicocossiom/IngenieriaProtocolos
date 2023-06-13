using System.Net;
using System.Net.Sockets;

namespace UdpChat.Lib
{
    /// <summary>
    /// An abstract class that represents a sendable object in the chat system. For example a <see cref="Request"/> or a <see cref="Response"/>.
    /// </summary>
    public abstract class ChatSendable
    {
        /// <summary>
        /// Gets or sets the timestamp of the request.
        /// </summary>
        /// <value>The timestamp of the request.</value>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        /// <summary>
        /// Serializes the object and sends it to the specified endpoint using the specified client.
        /// </summary>
        /// <remarks>
        /// This method is virtual so that it can be overridden.
        /// The default implementation uses <see cref="System.Text.Json.JsonSerializer.Serialize(object?, Type, System.Text.Json.JsonSerializerOptions?)"/>.
        /// This makes it possible for any class that inherits from <see cref="ChatSendable"/> to be serialized and sent.
        /// </remarks>
        /// <param name="endpoint">The endpoint to send this ChatSendable to</param>
        /// <param name="sender">The UdpClient sending this ChatSendable</param>
        /// <returns>The numbers of bytes sent</returns>
        public virtual int SerializeAndSend(ref IPEndPoint endpoint, ref UdpClient sender)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this, GetType());
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return sender.Send(bytes, bytes.Length, endpoint);
        }
    }
}