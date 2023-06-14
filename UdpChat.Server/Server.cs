using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using Microsoft.Data.Sqlite;
using UdpChat.Lib.Authentication;
using UdpChat.Lib.Message;
using UdpChat.Server.CLI;
using UdpChat.Server.Exceptions;

namespace UdpChat.Server
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
    /// The central retransmission server for the chat system. It handles auth requests and retransmission of messages.
    /// </summary>
    public class CentralRetransmissionServer
    {
        private int authenticationPort { get; set; } = 5000;
        private int retransmissionPort { get; set; } = 5001;
        private string dbPath { get; set; } = "users.db";
        private SqliteConnection? dbConnection { get; set; }
        /// <inheritdoc/>
        public ServerService authenticationService { get; set; }
        /// <inheritdoc/>
        public ServerService retransmissionService { get; set; }
        private static readonly Dictionary<string, IPEndPoint> clients = new Dictionary<string, IPEndPoint>();



        /// <summary>
        /// Creates a new retranmission server with the specified ports
        /// </summary>
        /// <param name="registerPort"></param>
        /// <param name="retransmissionPort"></param>
        public CentralRetransmissionServer(int registerPort = 5000, int retransmissionPort = 5001) : base()
        {
            Console.WriteLine($"Creating server with ports {registerPort} and {retransmissionPort}");
            this.authenticationPort = registerPort;
            this.retransmissionPort = retransmissionPort;
        }

        private async void InitializeDB()
        {
            var conString = new SqliteConnectionStringBuilder();
            conString.DataSource = this.dbPath;
            conString.Mode = SqliteOpenMode.ReadWriteCreate;
            conString.Cache = SqliteCacheMode.Shared;
            try
            {
                Console.WriteLine("Opening server database");
                dbConnection = new SqliteConnection(conString.ConnectionString);
                await dbConnection.OpenAsync();
                var cmd = dbConnection.CreateCommand();
                cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS users(
                username TEXT PRIMARY KEY,
                password TEXT,
                ip_address TEXT,
                port INTEGER
            );";
                var changes = cmd.ExecuteNonQuery();
                Console.WriteLine($"Users table created with {changes} changes");
            }
            catch (SqliteException e)
            {
                Console.WriteLine("Error creating database");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <exception cref="ChatDatabaseNotInitializedException"></exception>
        /// <exception cref="UserAlreadyRegisteredException"></exception>
        public async Task RegisterUserAsync(String name, String password, String ipAddress, int port)
        {
            if (dbConnection == null)
            {
                Console.WriteLine("Database not initialized");
                throw new ChatDatabaseNotInitializedException();
            }
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(ipAddress))
            {
                Console.WriteLine("One or more required string parameters are null or empty");
                throw new ArgumentNullException();
            }

            if (port <= 0)
            {
                Console.WriteLine("Invalid port number");
                throw new ArgumentOutOfRangeException();
            }
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = "INSERT INTO users (username, password, ip_address, port) VALUES (@username, @password, @ip_address, @port)";
            cmd.Parameters.AddWithValue("@username", name);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@ip_address", ipAddress);
            cmd.Parameters.AddWithValue("@port", port);
            Console.WriteLine($"Register SQL operation {cmd.CommandText} {cmd.Parameters.ToString()}");
            try { await cmd.ExecuteNonQueryAsync(); }
            catch (SqliteException)
            {
                // Console.Error.WriteLine($"SQL error {cmd.CommandText} {cmd.Parameters.ToString()}");
                throw new UserAlreadyRegisteredException($"User {name} already exists");
            }
        }
        /// <summary>
        /// Async database query to check if a user exists and has the correct password
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns>An awaitable task which resolves to true if the user exists and has the correct password, false otherwhise</returns>
        /// <exception cref="ChatDatabaseNotInitializedException"></exception>
        public async Task<bool> LoginUserAsync(string name, string password)
        {
            if (dbConnection == null)
            {
                Console.WriteLine("Database not initialized");
                throw new ChatDatabaseNotInitializedException();
            }
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM users WHERE username = @username AND password = @password";
            cmd.Parameters.AddWithValue("@username", name);
            cmd.Parameters.AddWithValue("@password", password);
            var reader = await cmd.ExecuteReaderAsync();
            return reader.HasRows;
        }
        /// <summary>
        /// Sets the <see cref="CentralRetransmissionServer.authenticationService"/> This service is responsible for all authentication operations.
        /// </summary>
        ///<remarks>
        /// Starts a <see cref="ServerService"/> in localhost (127.0.0.1)with the specified port <see cref="CentralRetransmissionServer.authenticationPort"/>.
        /// </remarks>

        public void StartAuthenticationService()
        {
            var authSocket = new UdpClient(authenticationPort);
            var authEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), authenticationPort);
            Console.WriteLine($"Authentication server started at {authEndpoint}");
            this.authenticationService = new ServerService(authSocket, authEndpoint);
        }

        /// <summary>
        /// Sets the <see cref="CentralRetransmissionServer.retransmissionService"/>. This service is responsible for receiving messages from clients and retransmitting them to the all registered users.
        /// </summary>
        ///<remarks>
        /// Starts a <see cref="ServerService"/> in localhost (127.0.0.1)with the specified port <see cref="CentralRetransmissionServer.retransmissionPort"/>.
        /// </remarks>

        public void StartRetransmissionService()
        {
            var retransmissionSocket = new UdpClient(retransmissionPort);
            var retransmisionEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), retransmissionPort);
            Console.WriteLine($"Retransmission server started at {retransmisionEndpoint}");
            this.retransmissionService = new ServerService(retransmissionSocket, retransmisionEndpoint);
        }

        /// <summary>
        /// Stops the services and closes the database connection
        /// </summary>
        public void Stop()
        {
            Console.WriteLine("Stopping server and related services");
            authenticationService.socket.Close();
            retransmissionService.socket.Close();
            Console.WriteLine("Closing database connection");
            dbConnection?.Close();
            Console.WriteLine("Exited successfully");
            System.Environment.Exit(0);
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


        /// <summary>
        /// Handles the registration of a new user
        /// </summary>
        /// <param name="res"></param>
        private void RetransmissionHandler(IAsyncResult res)
        {
            if (!TryGetStateFromAsyncRes(ref res, out var clientState)) return;
            var receiveBytes = clientState.socket.EndReceive(res, remoteEP: ref clientState.endpoint!);
            Task.Run(async () =>
            {
                var message = System.Text.Json.JsonSerializer.Deserialize<ChatMessage>(receiveBytes);
                if (message == null)
                {
                    Console.Error.WriteLine("Error deserializing message");
                    new ChatMessageResponse(false, 0).SerializeAndSend(ref clientState.endpoint, ref clientState.socket);
                    return;
                }
                Console.WriteLine($"Received message from {message.User.Username}@{clientState.endpoint} at {message.Timestamp}:\n\t{message.Message}");
                Console.WriteLine($"Retransmitting message to all registered users");
                var usersEndpoints = await this.GetRegisteredUsersAndEndpointsAsync();
                foreach (var (user, endpoint) in usersEndpoints)
                {
                    Console.WriteLine($"Retransmitting message to {endpoint}");
                    var sentBytesRetransmission = message.SerializeAndSend(endpoint: endpoint, ref clientState.socket);
                    Console.WriteLine($"Sent {sentBytesRetransmission} bytes to {endpoint}");
                }

                // send the message to all clients
                var sentBytes = new ChatMessageResponse(true, usersEndpoints.Count()).SerializeAndSend(ref clientState.endpoint, ref clientState.socket);
                Console.WriteLine($"Sent {sentBytes} bytes to {clientState.endpoint}");
            });
            // Start a new receive operation
            clientState.socket.BeginReceive(new AsyncCallback(RetransmissionHandler), clientState);
        }

        /// <summary>
        /// Retrieves the latest endpoints of all registered users from the database
        /// </summary>
        /// <returns>Returns a list of pairs from the database containing the username and the endpoint of the user</returns>
        /// <exception cref="Exception"></exception>
        private async Task<IEnumerable<(String, IPEndPoint)>> GetRegisteredUsersAndEndpointsAsync()
        {
            // get all users from the database
            if (dbConnection == null)
            {
                Console.WriteLine("Database not initialized");
                throw new Exception("Database not initialized");
            }

            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = "SELECT * FROM users";
            var reader = await cmd.ExecuteReaderAsync();

            // get the schema of this table
            var schema = reader.GetColumnSchema().ToList();

            // get the index of the user, ip_address and port columns
            var ipIndex = schema.FindIndex(col => col.ColumnName == "ip_address");
            var portIndex = schema.FindIndex(col => col.ColumnName == "port");
            var nameIndex = schema.FindIndex(col => col.ColumnName == "username");


            var usersEndpoints = new List<(String, IPEndPoint)>();

            foreach (var user in reader)
            {
                var username = reader.GetString(nameIndex);
                var ip = reader.GetString(ipIndex);
                var port = reader.GetInt32(portIndex);
                usersEndpoints.Add(
                    (
                        username,
                        new IPEndPoint(IPAddress.Parse(ip), port)
                        ));
            }

            return usersEndpoints;
        }


        private async Task HandleRegisterRequestAsync(Request req, ServerService clientState)
        {
            Response regRes;
            try
            {
                await this.RegisterUserAsync(req.Username, req.Password,
                clientState.endpoint.Address.ToString(), clientState.endpoint.Port);
                regRes = new Response(responseState: Response.State.REGISTER_SUCCESS, "User registered successfully");
            }
            catch (UserAlreadyRegisteredException e)
            {
                Console.WriteLine($"\tError registering user {req.Username}: {e.Message}");
                regRes = new Response(responseState: Response.State.ALREADY_REGISTERED, $"{e.Message}, please login instead");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\tError registering user {req.Username}: {e.Message}");
                regRes = new Response(responseState: Response.State.ERROR, $"Unknown error: {e.Message}");
            }
            Console.WriteLine($"\tSending register response {regRes} to {clientState.endpoint}");
            var sentBytes = regRes.SerializeAndSend(clientState.endpoint, this.authenticationService.socket);
            Console.WriteLine($"\tSent {sentBytes} bytes");
        }

        private async Task HandleLoginRequestAsync(Request req, ServerService clientState)
        {
            Console.WriteLine($"Received login request from {req.Username} at {req.Timestamp}");
            Response res = await LoginUserAsync(req.Username, req.Password) ?
                        new Response(responseState: Response.State.LOGIN_SUCCESS, "User logged in successfully")
                        :
                        new Response(responseState: Response.State.LOGIN_FAILED, "Invalid username or password");
            var sentBytes = res.SerializeAndSend(ref clientState.endpoint, ref clientState.socket);
            Console.WriteLine($"\tSent {sentBytes} bytes");
        }


        private void AuthenticationHandler(IAsyncResult res)
        {
            if (!TryGetStateFromAsyncRes(ref res, out var clientState)) return;
            byte[] receiveBytes = clientState.socket.EndReceive(res, ref clientState.endpoint!);
            Task.Run(async () =>
            {
                var req = System.Text.Json.JsonSerializer.Deserialize<Request>(receiveBytes);
                Console.WriteLine($"Received request from {clientState.endpoint}:\n\t{req}");
                if (req == null)
                {
                    Console.Error.WriteLine("\tError deserializing request");
                    return;
                }
                switch (req.Type)
                {
                    case Request.RequestType.REGISTER:
                        await HandleRegisterRequestAsync(req, clientState);
                        break;
                    case Request.RequestType.LOGIN:
                        await HandleLoginRequestAsync(req, clientState);
                        break;
                    default:
                        Console.Error.WriteLine($"Invalid request type {req.Type}");
                        new Response(Response.State.ERROR, $"Invalid request type {req.Type}")
                            .SerializeAndSend(ref clientState.endpoint, ref clientState.socket);
                        break;
                }
            });
            // Start a new receive operation
            clientState.socket.BeginReceive(new AsyncCallback(AuthenticationHandler), clientState);
        }
        /// <summary>
        /// Starts the server with the associated services:
        /// </summary>
        /// <remarks>
        /// The services started are:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="CentralRetransmissionServer.authenticationService"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="CentralRetransmissionServer.retransmissionService"/></term>
        /// </item>
        /// </list>
        /// </remarks>
        public void Start()
        {
            Console.WriteLine("Starting server");
            InitializeDB();
            StartRetransmissionService();
            StartAuthenticationService();
            this.retransmissionService.socket.BeginReceive(new AsyncCallback(RetransmissionHandler), this.retransmissionService);
            this.authenticationService.socket.BeginReceive(new AsyncCallback(AuthenticationHandler), this.authenticationService);
            Console.CancelKeyPress += delegate
            {
                Stop();
            };
            while (true)
            { }
        }
        /// <summary>
        /// The entry point of the server. Starts a <see cref="CentralRetransmissionServer"/> with default settings and associated services.
        /// </summary>
        static void Main(string[] args)
        {
            var port = ServerCLI.ParseArgs(args) ?? 5000;
            CentralRetransmissionServer server = new CentralRetransmissionServer(port, port + 1);
            server.Start();
        }
    }
}