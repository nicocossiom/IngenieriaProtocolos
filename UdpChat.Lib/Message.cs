using System.Net;
using System.Net.Sockets;

namespace UdpChat
{
    /// <summary>
    /// A ChatUser is a user of the chat system. It has a username and two endpoints, one for sending and one for receiving.
    /// </summary>
    [Serializable]
    public class ChatUser
    {
        /// <inheritdoc/>
        public string Username { get; set; }
        /// <inheritdoc/>
        public string Password { get; set; }
        /// <inheritdoc/>
        public ChatUser(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }
    }
    /// <summary>
    /// A ChatMessage is a message sent by a user to the server. It has a user, a timestamp, and a message.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        /// <inheritdoc/>
        public ChatUser User { get; set; }
        /// <inheritdoc/>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        /// <inheritdoc/>
        public String Message { get; set; }
        /// <inheritdoc/>
        public ChatMessage(ChatUser user, string message)
        {
            this.User = user;
            this.Message = message;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ChatMessage: {User.Username} at {Timestamp}: {Message}";
        }
        /// <inheritdoc/>
        public int SerializeAndSend(ref IPEndPoint endpoint, ref UdpClient client)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return client.Send(bytes, bytes.Length, endpoint);
        }

    }


    /// <summary>
    /// The responnse to a ChatMessage from the server.
    /// </summary>
    [Serializable]
    public class ChatMessageResponse
    {
        /// <value> If true means the server received the message correctly</value>
        public Boolean ReceivedCorrectly { get; set; }
        /// <inheritdoc/>
        public ChatMessageResponse(Boolean receivedCorrectly)
        {
            this.ReceivedCorrectly = receivedCorrectly;
        }
        /// <inheritdoc/>

        public int SerializeAndSend(ref IPEndPoint endpoint, ref UdpClient client)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return client.Send(bytes, bytes.Length, endpoint);
        }
        /// <inheritdoc/>

        public override string ToString()
        {
            return $"ChatMessageResponse: {ReceivedCorrectly}";
        }
    }
}