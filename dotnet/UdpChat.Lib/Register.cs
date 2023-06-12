using System.Net;
using System.Net.Sockets;

namespace UdpChat;
/// <summary>
/// Request to register a user to the server. It has a username and a timestamp.
/// </summary>
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

    public int SerializeAndSend(ref IPEndPoint endpoint, ref UdpClient client)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(this);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return client.Send(bytes, bytes.Length, endpoint);
    }
}

/// <summary>
/// Response from the server to a RegisterRequest from the user. It has a message and a response state.
/// </summary>
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