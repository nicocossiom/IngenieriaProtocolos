namespace UdpChat.Lib.Message
{
    using UdpChat.Lib.ChatUser;

    /// <summary>
    /// A ChatMessage is a message sent by a user to the server. It has a user, a timestamp, and a message.
    /// </summary>
    [Serializable]
    public class ChatMessage : ChatSendable
    {
        /// <inheritdoc/>
        public ChatUser User { get; set; }
        /// <inheritdoc/>
        public new DateTime Timestamp { get; set; } = DateTime.Now;
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



    }

    /// <summary>
    /// The responnse to a ChatMessage from the server.
    /// </summary>
    [Serializable]
    public class ChatMessageResponse : ChatSendable
    {
        /// <value> If true means the server received the message correctly</value>
        public Boolean Received { get; set; }
        /// <value> If true means the server received the message correctly</value>
        public int RetransmittedNTimes { get; set; }
        /// <inheritdoc/>
        public ChatMessageResponse(Boolean received, int retransmittedNTimes)
        {
            this.Received = received;
            this.RetransmittedNTimes = retransmittedNTimes;
        }

        /// <inheritdoc/>

        public override string ToString()
        {
            return $"ChatMessageResponse(Received:{Received}, RetransmittedNTimes:{RetransmittedNTimes})";
        }
    }
}