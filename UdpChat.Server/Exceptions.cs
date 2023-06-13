class UserAlreadyRegisteredException : Exception
{
    public UserAlreadyRegisteredException(string message) : base(message) { }
}