using System.Net;
using System.Net.Sockets;

namespace UdpChat
{
    [Serializable]
    public class ChatMessage
    {
        public string username { get; set; }
        public DateTime timestamp { get; set; } = DateTime.Now;
        public String message { get; set; }
        public ChatMessage(string username, string message)
        {
            this.username = username;
            this.message = message;
        }
        public override string ToString()
        {
            return $"ChatMessage: {username} at {timestamp}: {message}";
        }
        public int SerializeAndSend(ref IPEndPoint endpoint, ref UdpClient client)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return client.Send(bytes, bytes.Length, endpoint);
        }

    }


    [Serializable]
    public class ChatMessageResponse
    {
        public Boolean successOnRetransmission { get; set; }
        public ChatMessageResponse(Boolean successOnRetransmission)
        {
            this.successOnRetransmission = successOnRetransmission;
        }

        public int SerializeAndSend(ref IPEndPoint endpoint, ref UdpClient client)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return client.Send(bytes, bytes.Length, endpoint);
        }

        public override string ToString()
        {
            return $"ChatMessageResponse: {successOnRetransmission}";
        }
    }
}