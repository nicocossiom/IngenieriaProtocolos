using System.Net;
using System.Net.Sockets;
using UdpChat;
class ChatClient
{
    private UdpClient recieveSocket;
    private UdpClient sendSocket;
    private IPEndPoint centralServerMessageEndpoint;
    private IPEndPoint centralServerRegisterEndpoint;

    public ChatClient(int socket = 4000, string centralServerIP = "127.0.0.1", int centralServerPort = 5000) : base()
    {
        this.centralServerMessageEndpoint = new IPEndPoint(IPAddress.Parse(centralServerIP), centralServerPort + 1);
        this.centralServerRegisterEndpoint = new IPEndPoint(IPAddress.Parse(centralServerIP), centralServerPort);
        this.recieveSocket = new UdpClient(socket);
        this.sendSocket = new UdpClient(socket + 1);
        Console.WriteLine($"Client created at {socket} with central server at {centralServerIP}:{centralServerPort}");
    }

    public void SendMessage(String username, String msg)
    {
        ChatMessage message = new ChatMessage(username, msg);
        message.SerializeAndSend(ref centralServerMessageEndpoint, ref sendSocket);
        // await a response from the server
        Console.WriteLine($"Waiting for response from server {centralServerMessageEndpoint}...");
        var response = sendSocket.Receive(ref centralServerMessageEndpoint);
        // deserialize the response into a ChatMessageResponse
        var responseMessage = System.Text.Json.JsonSerializer.Deserialize<ChatMessageResponse>(response);
        if (responseMessage == null)
        {
            Console.WriteLine("Error deserializing response");
            return;
        }
        Console.WriteLine($"Message retransmitted: {responseMessage.successOnRetransmission}");
    }
    // public void SendRegister(String username)
    // {
    //     var register = new RegisterRequest(username);
    //     register.serializeAndSend(ref centralServerRegisterEndpoint, ref sendSocket);
    //     // await a response from the server
    //     Console.WriteLine($"Waiting for response from server {centralServerRegisterEndpoint}...");
    //     var response = sendSocket.Receive(ref centralServerRegisterEndpoint);
    //     // deserialize the response into a RegisterResponse
    //     var responseMessage = System.Text.Json.JsonSerializer.Deserialize<RegisterResponse>(response);
    //     if (responseMessage == null)
    //     {
    //         Console.WriteLine("Error deserializing response");
    //         return;
    //     }
    //     Console.WriteLine($"Server response: {responseMessage.message} {responseMessage.responseState}");
    // }
}

class Program
{
    static void Main(string[] args)
    {
        Boolean useDefault = true;
        ChatClient client;
        Console.WriteLine("Welcome to the UDP chat client!");
        while (true)
        {
            Console.WriteLine("Do you want to use the default settings? (y/n)");
            var input = Console.ReadLine();
            if (input != "y" && input != "n")
            {
                Console.WriteLine("Invalid input");
                continue;
            }
            if (input == "y" || input == "Y")
            {
                useDefault = true;
                break;
            }
        }
        if (!useDefault)
        {
            Console.WriteLine("Enter a port for the client (enter for 4000):");
            var port = Console.ReadLine();
            Console.WriteLine("IP of central server (enter for 127.0.0.1):");
            var centralServerAdress = Console.ReadLine();
            Console.WriteLine("Port of central server (enter for 5000):");
            var centralServerPort = Console.ReadLine();
            Console.WriteLine($"Client config:\n\tClient ports: Recieve{port} - Send {port + 1}\n\tCentral Server: Adress{centralServerAdress} - Port {centralServerPort}");
            client = new ChatClient(int.Parse(port), centralServerAdress, int.Parse(centralServerPort));
        }
        else
        {
            client = new ChatClient();
        }
        Console.WriteLine("Enter a username:");
        var username = Console.ReadLine();
        while (true)
        {
            Console.WriteLine("Available commands are 'send' and 'register'");
            var command = Console.ReadLine();
            switch (command)
            {
                case "send":
                    Console.WriteLine("Enter a message:");
                    var msg = Console.ReadLine();
                    client.SendMessage(username, msg);
                    break;
                // case "register":
                //     client.SendRegister(username);
                //     break;
                default:
                    Console.WriteLine("Invalid command");
                    break;
            }

        }
    }
}
