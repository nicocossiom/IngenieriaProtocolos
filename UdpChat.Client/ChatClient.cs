using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UdpChat.Lib.ChatUser;
using UdpChat.Lib.Message;
using UdpChat.Lib.Authentication;

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
            sendSocket.Client.ReceiveTimeout = 5000;
            sendSocket.Client.SendTimeout = 5000;
            receiveSocket.Client.ReceiveTimeout = 5000;
            receiveSocket.Client.SendTimeout = 5000;
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
            var msgResponse = System.Text.Json.JsonSerializer.Deserialize<ChatMessageResponse>(response);
            if (msgResponse == null)
            {
                Console.Error.WriteLine("Error deserializing response");
                return;
            }
            if (msgResponse.Received)
                Console.WriteLine($"Message was received correctly and retransmitted (with no garantees of reaching) to {msgResponse.RetransmittedNTimes} clients");
            else
                Console.WriteLine($"Message was rec");
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
                if (res.ResponseState == Response.State.LOGIN_SUCCESS)
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
        }

    }
}
