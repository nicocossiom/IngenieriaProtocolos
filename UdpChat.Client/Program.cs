using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UdpChat.Lib;
namespace UdpChat.Client
{

    /// <summary>
    /// A ChatClient is a client for the chat system.
    /// </summary>
    public class ChatClient
    {
        /// <value>UDP socket of this ChatClient instance used to receive messages</value>
        private UdpClient receiveSocket;
        /// <value>UDP socket of this ChatClient used to send messages</value>
        private UdpClient sendSocket;
        /// <value>ChatUser instance representing this ChatClient</value>
        public ChatUser? user;
        /// <value>IPEndPoint of the central server used to send messages. Contains port and address</value>
        private IPEndPoint centralServerMessageEndpoint;
        /// <value>IPEndPoint of the central server used to send messages. Contains port and address</value>
        private IPEndPoint centralServerRegisterEndpoint;
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatClient"/> class with the specified client port and central server IP and port.
        /// </summary>
        /// <param name="clientPort">The port number to use for the client.</param>
        /// <param name="centralServerIP">The IP address of the central server.</param>
        /// <param name="centralServerPort">The port number to use for the central server.</param>
        /// <exception cref="ArgumentException">Thrown if the receive socket's local endpoint is null.</exception>
        public ChatClient(int clientPort = 4000, string centralServerIP = "127.0.0.1", int centralServerPort = 5000) : base()
        {
            this.centralServerMessageEndpoint = new IPEndPoint(IPAddress.Parse(centralServerIP), centralServerPort + 1);
            this.centralServerRegisterEndpoint = new IPEndPoint(IPAddress.Parse(centralServerIP), centralServerPort);
            this.receiveSocket = new UdpClient(clientPort);
            this.sendSocket = new UdpClient(clientPort + 1);
            if (receiveSocket.Client.LocalEndPoint == null || sendSocket.Client.LocalEndPoint == null)
                throw new ArgumentException("recieveSocket.Client.LocalEndPoint is null");
            this.user = null;
            Console.WriteLine($"Client created at {clientPort} with central server at {centralServerIP}:{centralServerPort}");
        }

        /// <summary>
        /// Pings the central server to check if it is reachable 
        /// </summary>
        /// <returns> true if the central server can be reached, false if not</returns>
        public Boolean PingCentralServer()
        {
            // send a ping to the central server, if it responds, return true, else return false
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(centralServerMessageEndpoint.Address);
            // Console.WriteLine($"Ping to {centralServerMessageEndpoint.Address} returned {reply.Status}");
            return reply.Status == IPStatus.Success;
        }

        /// <summary>
        /// Sends a message if the user is logged in to the central server.
        /// </summary>
        /// <param name="msg"></param>

        public void SendMessage(String msg)
        {
            if (this.user == null)
            {
                Console.Error.WriteLine("You must be logged in to send a message");
                return;
            }
            ChatMessage message = new ChatMessage(this.user, msg);
            message.SerializeAndSend(ref centralServerMessageEndpoint, ref sendSocket);
            // await a response from the server
            Console.WriteLine($"Waiting for response from server {centralServerMessageEndpoint}...");
            var response = sendSocket.Receive(ref centralServerMessageEndpoint);
            // deserialize the response into a ChatMessageResponse
            var responseMessage = System.Text.Json.JsonSerializer.Deserialize<ChatMessageResponse>(response);
            if (responseMessage == null)
            {
                Console.Error.WriteLine("Error deserializing response");
                return;
            }
            Console.WriteLine($"Message retransmitted: {responseMessage.ReceivedCorrectly}");
        }
        /// <summary>
        /// Sends a an auth request of type <see cref="Request.RequestType"/> to the central server.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="type"></param>
        public void SendAuthRequest(string username, string password, Request.RequestType type)
        {
            var login = new Request(username, password, type);
            login.SerializeAndSend(ref centralServerRegisterEndpoint, ref sendSocket);
            // await a response from the server
            Console.WriteLine($"Waiting for response from server {centralServerRegisterEndpoint}...");
            var resSerialized = sendSocket.Receive(ref centralServerRegisterEndpoint);
            // deserialize the response into a LoginResponse
            var res = System.Text.Json.JsonSerializer.Deserialize<Response>(resSerialized);
            if (res == null)
            {
                Console.WriteLine("Error deserializing response");
                return;
            }

            Console.WriteLine(res.Message);
            if (res.ResponseState == Response.State.LOGIN_SUCCESS)
            {
                this.user = new ChatUser(username, password);
                Console.WriteLine($"Currently logged in as {username}");
            }
        }
    }

    /// <summary>
    /// The program class, contains the entry point of the program for the client application
    /// </summary>
    public class Program
    {
        /// <summary> Returns a ChatClient with a custom configuration from user input </summary>
        /// <remarks> When called this method will ask the user for a port for the client, an IP for the central server and a port for the central server </remarks>
        /// <returns> A ChatClient instance, either with the specified settings or custom settings</returns> 
        private static ChatClient ChatClientFromInputCustomConfig()
        {
            Console.WriteLine("Enter a port for the client (enter for 4000):");
            var port = Console.ReadLine();
            Console.WriteLine("IP of central server (enter for 127.0.0.1):");
            var centralServerAdress = Console.ReadLine();
            Console.WriteLine("Port of central server (enter for 5000):");
            var centralServerPort = Console.ReadLine();
            if (string.IsNullOrEmpty(port) || string.IsNullOrEmpty(centralServerAdress) || string.IsNullOrEmpty(centralServerPort))
            {
                Console.Error.WriteLine("Invalid input, retrying...");
                return ChatClientFromInputCustomConfig();
            }
            if (!int.TryParse(port, out int portInt) || !int.TryParse(centralServerPort, out int centralServerPortInt))
            {
                Console.Error.WriteLine("Invalid ports, retrying...");
                return ChatClientFromInputCustomConfig();
            }
            string settingsString = $@"
            Client config:
            Client ports: Recieve{port} - Send {port + 1}
            Central Server: Adress{centralServerAdress} - Port {centralServerPort}
            ";
            Console.WriteLine(settingsString);

            Console.WriteLine("Is this correct? (y/n)");
            var input = Console.ReadLine();
            while (!string.IsNullOrEmpty(input) && "yYnN".IndexOf(input) == -1)
            {
                Console.Error.WriteLine("Invalid input. Options are y/n");
                Console.WriteLine(settingsString);
                Console.WriteLine("Is this correct? (y/n)");
                input = Console.ReadLine();
            }
            if (input == "n" || input == "N") return ChatClientFromInputCustomConfig();
            return new ChatClient(portInt, centralServerAdress, centralServerPortInt);
        }
        /// <summary>
        /// Asks the user if they want to use the default settings or a custom configuration
        /// </summary>
        /// <remarks>
        /// Default settings:
        /// Client ports: Recieve 4000 - Send 4001
        /// Central Server: Adress 127.0.0.1 - Port 5000
        /// </remarks>
        /// 
        /// <returns> A ChatClient instance, either with default or custom settings </returns>
        private static ChatClient ChatClientFromInput()
        {
            string settingsString = @"
        Default settings:
        Client ports: Recieve 4000 - Send 4001
        Central Server: Adress 127.0.0.1 - Port 5000
        ";
            while (true)
            {
                Console.WriteLine(settingsString);
                Console.WriteLine("Do you want to use the default settings? (y/n) (↩️  after input))");
                var input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input) && "yYnN".IndexOf(input) == -1)
                {
                    Console.WriteLine("Invalid input, retrying...");
                    continue;
                }
                if (input == "y" || input == "Y")
                    return new ChatClient();
                else if (input == "n" || input == "N")
                    return ChatClientFromInputCustomConfig();
                else
                {
                    Console.WriteLine("Invalid input, retrying...");
                    continue;
                }
            }
        }

        /// <summary>
        /// Asks the user for a username and password until correct values are entered
        /// </summary>
        /// <returns> Returns a tuple of (username, password)</returns>
        private static (string, string) GetUsernameAndPassword()
        {
            Console.WriteLine("Enter a username, spaces are allowed, but not empty string: (↩️  after input)");
            var username = Console.ReadLine();
            while (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Invalid username, re-enter:");
                username = Console.ReadLine();
            }
            Console.WriteLine("Enter a password, spaces are allowed, but not empty string: (↩️  after input)");
            var password = Console.ReadLine();
            while (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Invalid password, re-enter:");
                password = Console.ReadLine();
            }
            return (username, password);
        }
        /// <inheritdoc/>
        public void Main(string[] args)
        {
            Console.WriteLine("Welcome to the UDP chat client!");
            ChatClient client = ChatClientFromInput();
            if (!client.PingCentralServer())
            {
                Console.WriteLine("Could not reach central server. The server is no online. Exiting...");
                return;
            }
            while (true)
            {
                Console.WriteLine(@"Input a command, available commands are 
            - register
            - login
            - send 
            - unregister
            ");
                var command = Console.ReadLine();
                switch (command)
                {
                    case "send":
                        if (client.user == null)
                        {
                            Console.Error.WriteLine("You must be logged in to send a message");
                            continue;
                        }
                        Console.WriteLine("Enter a message: (↩️  to send, shift +  ↩️  for new line)");
                        var msg = Console.ReadLine();
                        while (string.IsNullOrEmpty(msg))
                        {
                            Console.WriteLine("Invalid message, try again");
                            msg = Console.ReadLine();
                        }
                        client.SendMessage(msg);
                        break;
                    case "register":
                        var (username, password) = GetUsernameAndPassword();
                        client.SendAuthRequest(username, password, Request.RequestType.REGISTER);
                        break;
                    case "login":
                        (username, password) = GetUsernameAndPassword();
                        client.SendAuthRequest(username, password, Request.RequestType.LOGIN);
                        break;
                    case "unregister":
                        if (client.user == null)
                        {
                            Console.Error.WriteLine("You must be logged as the user you want to unregister");
                            continue;
                        }
                        // TODO
                        Console.WriteLine("Unimplemented method 'unregister'");
                        // client.SendRegister(username);
                        break;
                    default:
                        Console.WriteLine("Invalid command");
                        break;
                }

            }
        }
    }
}
