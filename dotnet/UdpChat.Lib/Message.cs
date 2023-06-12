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
        public string username { get; set; }
        public EndPoint sendEndpoint { get; set; }
        public EndPoint receiveEndpoint { get; set; }
        public ChatUser(string username, string address, int port)
        {
            this.username = username;
            this.receiveEndpoint = new EndPoint(address, port);
            this.sendEndpoint = new EndPoint(address, port + 1);
        }
    }
    /// <summary>
    /// A ChatMessage is a message sent by a user to the server. It has a user, a timestamp, and a message.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        public ChatUser user { get; set; }
        public DateTime timestamp { get; set; } = DateTime.Now;
        public String message { get; set; }
        public ChatMessage(ChatUser user, string message)
        {
            this.user = user;
            this.message = message;
        }
        public override string ToString()
        {
            return $"ChatMessage: {user.username} at {timestamp}: {message}";
        }
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
        public Boolean receivedCorrectly { get; set; }
        public ChatMessageResponse(Boolean successOnRetransmission)
        {
            this.receivedCorrectly = successOnRetransmission;
        }

        public int SerializeAndSend(ref IPEndPoint endpoint, ref UdpClient client)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return client.Send(bytes, bytes.Length, endpoint);
        }

        public override string ToString()
        {
            return $"ChatMessageResponse: {receivedCorrectly}";
        }
    }
}