namespace UdpChat.Server.Exceptions
{
    /// <inheritdoc/>
    public class UserAlreadyRegisteredException : Exception
    {
        /// <inheritdoc/>
        public UserAlreadyRegisteredException(string message) : base(message) { }
    }

    /// <inheritdoc/>
    public class UserAlreadyLoggedInxception : Exception
    {
        /// <inheritdoc/>
        public UserAlreadyLoggedInxception(string message) : base(message) { }
    }
    /// <inheritdoc/>

    public class ChatDatabaseNotInitializedException : Exception
    {
        /// <inheritdoc/>
        public ChatDatabaseNotInitializedException() : base("Database not initialized") { }
    }
}
