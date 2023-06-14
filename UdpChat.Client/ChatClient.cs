using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UdpChat.Lib.ChatUser;
using UdpChat.Lib.Message;
using UdpChat.Lib.Authentication;
using UdpChat.Lib;

namespace UdpChat.Client
{

    /// <summary>
    /// A struct to hold the state of a UdpClient
    /// </summary>
    public struct ServerService
    {
        /// <summary>
        /// The socket of the client
        /// </summary>
        public UdpClient socket;
        /// <summary>
        /// The endpoint of the client
        /// </summary>
        public IPEndPoint endpoint;

        /// <summary>
        /// A constructor to create a new ServerService with the specified client and endpoint. After a ServerService is created, it can be used to receive messages from the client.
        /// A handler for the received messages can be specified with the <see cref="UdpClient.BeginReceive(System.AsyncCallback, object)"/> method.
        /// As a handler can only accept a request at a time, it's recommended to use <see cref="System.Threading.Tasks.Task.Run(System.Action)"/> inside the handler to process the request inside.
        /// </summary>
        /// <example>
        /// <code>
        /// private void AuthenticationHandler(IAsyncResult res)
        /// {
        ///    if (!TryGetStateFromAsyncRes(ref res, out var clientState)) return;
        ///    byte[] receiveBytes = clientState.socket.EndReceive(res, ref clientState.endpoint!);
        ///    Task.Run(() =>
        ///    {
        ///        var req = System.Text.Json.JsonSerializer.Deserialize&lt;Request&gt;(receiveBytes);
        ///        Console.WriteLine($"Received request from {clientState.endpoint}:\n\t{req}");
        ///        if (req == null)
        ///        {
        ///            Console.Error.WriteLine("Error deserializing request");
        ///            return;
        ///        }
        ///        switch (req.Type)
        ///        {
        ///            case Request.RequestType.REGISTER:
        ///                HandleRegisterRequest(req, ref clientState);
        ///                break;
        ///            case Request.RequestType.LOGIN:
        ///                HandleLoginRequest(req, ref clientState);
        ///                break;
        ///            default:
        ///                Console.Error.WriteLine($"Invalid request type {req.Type}");
        ///                new Response(Response.State.ERROR, $"Invalid request type {req.Type}")
        ///                    .SerializeAndSend(ref clientState.endpoint, ref clientState.socket);
        ///                break;
        ///        }
        ///    });
        ///    // Start a new receive operation
        ///    clientState.socket.BeginReceive(new AsyncCallback(AuthenticationHandler), clientState);
        /// }
        /// </code>
        /// </example>
        /// <param name="client">The UdpClient used for network communications.</param>
        /// <param name="endpoint">The network endpoint where the service is located.</param>
        public ServerService(UdpClient client, IPEndPoint endpoint)
        {
            this.socket = client;
            this.endpoint = endpoint;
        }
    }


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
        /// <inheritdoc/>
        public ServerService MessageListenerService { get; set; }

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
            this.receiveSocket = new UdpClient(clientPort + 1);
            this.sendSocket = new UdpClient(clientPort);
            sendSocket.Client.ReceiveTimeout = 5000;
            sendSocket.Client.SendTimeout = 5000;
            receiveSocket.Client.ReceiveTimeout = 5000;
            receiveSocket.Client.SendTimeout = 5000;
            if (receiveSocket.Client.LocalEndPoint == null || sendSocket.Client.LocalEndPoint == null)
                throw new ArgumentException("recieveSocket.Client.LocalEndPoint is null");
            this.user = null;
            this.MessageListenerService = new ServerService(receiveSocket, (IPEndPoint)receiveSocket.Client.LocalEndPoint);
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
            var msgResponse = System.Text.Json.JsonSerializer.Deserialize<ChatMessageResponse>(response);
            if (msgResponse == null)
            {
                Console.Error.WriteLine("Error deserializing response");
                return;
            }
            Console.WriteLine($"Received response {msgResponse}");
            if (msgResponse.Received)
                Console.WriteLine($"Message was received correctly and retransmitted (with no garantees of reaching) to {msgResponse.RetransmittedNTimes} clients");
            else
                Console.WriteLine($"Message was received correctly but not retransmitted");
        }
        /// <summary>
        /// Sends a an auth request of type <see cref="Request.RequestType"/> to the central server.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="type"></param>
        public void SendAuthRequest(string username, string password, Request.RequestType type)
        {
            var req = new Request(username, password, type);
            Console.WriteLine($"Sending request {req} to {centralServerRegisterEndpoint}...");
            var sentBytes = req.SerializeAndSend(ref centralServerRegisterEndpoint, ref sendSocket);
            Console.WriteLine($"Sent {sentBytes}");
            // await a response from the server
            Console.WriteLine($"Waiting for response from server {centralServerRegisterEndpoint}...");
            try
            {
                var resSerialized = sendSocket.Receive(ref centralServerRegisterEndpoint);
                // deserialize the response into a LoginResponse
                var res = System.Text.Json.JsonSerializer.Deserialize<Response>(resSerialized);
                if (res == null)
                {
                    Console.WriteLine("Error deserializing response");
                    return;
                }

                Console.WriteLine(res.Message);
                if (res.ResponseState == Response.State.LOGIN_SUCCESS || res.ResponseState == Response.State.ALREADY_LOGGED_IN && this.user == null)
                {
                    this.user = new ChatUser(username, password);
                    Console.WriteLine($"Currently logged in as {username}");
                }
            }
            catch (SocketException e)
            {
                Console.Error.WriteLine($"{e.Message}. The server is either down or unreachable from you. Check your settings or try again later.");
                return;
            }
            catch (System.Text.Json.JsonException)
            {
                Console.Error.WriteLine("The server sent a response but it was not a valid one. Since the state of the server is unknown you are not logged in yet, please try again and see if you are logged in.");
            }
        }

        private bool TryGetStateFromAsyncRes(ref IAsyncResult res, out ServerService clientState)
        {
            clientState = default;
            if (res.AsyncState == null)
            {
                Console.Error.WriteLine("Invalid AsyncState. Unable to receive message.");
                return false;
            }
            clientState = (ServerService)res.AsyncState;
            if (clientState.socket == null || clientState.endpoint == null)
            {
                Console.Error.WriteLine("Invalid AsyncState. Unable to receive message.");
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public void ReceiveCallback(IAsyncResult ar)
        {
            Task.Run(() =>
            {
                Console.WriteLine("Received message from server");
                // Get the socket that handles the client request.
                if (!TryGetStateFromAsyncRes(ref ar, out ServerService clientState))
                    return Task.CompletedTask;
                // Read data from the client socket.
                var bytes = clientState.socket.EndReceive(ar, ref clientState.endpoint!);
                // Deserialize the message
                var msg = Deserializerer<ChatMessage>.Deserialize(bytes);
                if (msg == null)
                {
                    Console.Error.WriteLine("Error deserializing message");
                    return Task.CompletedTask;
                }
                Console.WriteLine($"Received message from {msg.User.Username}: {msg.Message}");
                this.MessageListenerService.socket.BeginReceive(new AsyncCallback(ReceiveCallback), clientState);
                return Task.CompletedTask;
            });
        }
        /// <inheritdoc/>

        public void StartMessageService()
        {
            Console.WriteLine("Messages that are sent by the server will be displayed, waiting for messages...");
            this.MessageListenerService.socket.BeginReceive(new AsyncCallback(ReceiveCallback), this.MessageListenerService);
        }
    }
}
