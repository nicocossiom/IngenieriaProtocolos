namespace UdpChat.Lib.ChatUser
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
}
