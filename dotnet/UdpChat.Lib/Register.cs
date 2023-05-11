namespace UdpChat;
public class RegisterRequest
{
    public string username { get; set; }
    public DateTime timestamp { get; set; } = DateTime.Now;

    public RegisterRequest(string username)
    {
        this.username = username;
    }
    public override string ToString()
    {
        return $"RegisterRequest: {username} at {timestamp}";
    }
}

public class RegisterResponse
{
    public string message { get; set; }
    public State responseState { get; set; }
    public enum State
    {
        OK,
        ALREADY_REGISTERED,
        ERROR
    }

    public RegisterResponse(State responseState, String message)
    {
        this.responseState = responseState;
        this.message = message;
    }

    public override string ToString()
    {
        return $"RegisterResponse: {responseState}";
    }

}