namespace UdpChat.Server
{
    class UserAlreadyRegisteredException : Exception
    {
        public UserAlreadyRegisteredException(string message) : base(message) { }
    }

    class ChatDatabaseNotInitializedException : Exception
    {
        public ChatDatabaseNotInitializedException() : base("Database not initialized") { }
    }
}
