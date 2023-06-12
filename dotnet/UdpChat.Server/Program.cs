using System.Net;
using System.Net.Sockets;
using Microsoft.Data.Sqlite;
using UdpChat;
public struct UdpState
{
    public UdpClient socket;
    public IPEndPoint endpoint;

    public UdpState(UdpClient client, IPEndPoint endpoint)
    {
        this.socket = client;
        this.endpoint = endpoint;
    }
}

class CentralRetransmissionServer
{
    private int registerPort { get; set; } = 5000;
    private int retransmissionPort { get; set; } = 5001;
    private string dbPath { get; set; } = "users.db";
    private SqliteConnection? dbConnection { get; set; }
    private UdpState registerServer { get; set; }
    private UdpState retransmissionServer { get; set; }
    private static readonly Dictionary<string, IPEndPoint> clients = new Dictionary<string, IPEndPoint>();


    // public CentralRetransmissionServer()
    public CentralRetransmissionServer(int registerPort = 5000, int retransmissionPort = 5001) : base()
    {
        Console.WriteLine($"Creating server with ports {registerPort} and {retransmissionPort}");
        this.registerPort = registerPort;
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
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY AUTOINCREMENT, username TEXT, address TEXT)";
            var changes = cmd.ExecuteNonQuery();
            Console.WriteLine($"Users table created with {changes} changes");
        }
        catch (SqliteException e)
        {
            Console.WriteLine("Error creating database");
            Console.WriteLine(e.Message);
        }
    }

    private void RegisterUser(String name, String address)
    {
        if (dbConnection == null)
        {
            Console.WriteLine("Database not initialized");
            throw new Exception("Database not initialized");
        }
        var cmd = dbConnection.CreateCommand();
        cmd.CommandText = "INSERT INTO users (username, address) VALUES (@username, @address)";
        cmd.Parameters.AddWithValue("@username", name);
        cmd.Parameters.AddWithValue("@address", address);
        if (cmd.ExecuteNonQuery() < 1)
        {
            throw new Exception("Error inserting user, probably already exists");
        }
    }

    private void StartRegisterServer()
    {
        var registerSocket = new UdpClient(registerPort);
        var registerEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), registerPort);
        Console.WriteLine($"Register server started at {registerEndpoint}");
        this.registerServer = new UdpState(registerSocket, registerEndpoint);
    }

    private void StartRetransmissionServer()
    {
        var retransmissionSocket = new UdpClient(retransmissionPort);
        var retransmisionEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), retransmissionPort);
        Console.WriteLine($"Retransmission server started at {retransmisionEndpoint}");
        this.retransmissionServer = new UdpState(retransmissionSocket, retransmisionEndpoint);
    }

    private void Stop()
    {
        Console.WriteLine("Stopping server");
        registerServer.socket.Close();
        retransmissionServer.socket.Close();
        dbConnection?.Close();
    }


    private void RetransmissionHandler(IAsyncResult res)
    {
        if (res.AsyncState == null)
        {
            Console.Error.WriteLine("Invalid AsyncState. Unable to receive message.");
            return;
        }
        UdpState clientState = (UdpState)res.AsyncState;
        if (clientState.socket == null || clientState.endpoint == null)
        {
            Console.Error.WriteLine("Invalid AsyncState. Unable to receive message.");
            return;
        }
        var receiveBytes = clientState.socket.EndReceive(res, ref clientState.endpoint!);
        var message = System.Text.Json.JsonSerializer.Deserialize<ChatMessage>(receiveBytes);
        if (message == null)
        {
            Console.Error.WriteLine("Error deserializing message");
            new ChatMessageResponse(false).SerializeAndSend(ref clientState.endpoint, ref clientState.socket);
            return;
        }
        Console.WriteLine($"Received message from {message.user.username}@{message.user.sendEndpoint} at {message.timestamp}:\n\t{message.message}");
        // send the message to all clients
        var sentBytes = new ChatMessageResponse(true).SerializeAndSend(ref clientState.endpoint, ref clientState.socket);
        Console.WriteLine($"Sent {sentBytes} bytes to {clientState.endpoint}");
    }


    private void RegisterHandler(IAsyncResult ar)
    {

        if (ar.AsyncState is UdpState clientState && clientState.socket != null && clientState.endpoint != null)
        {

            byte[] receiveBytes = clientState.socket.EndReceive(ar, ref clientState.endpoint!);
            var regReq = System.Text.Json.JsonSerializer.Deserialize<RegisterRequest>(receiveBytes);
            if (regReq == null)
            {
                Console.WriteLine("Error deserializing message");
                return;
            }
            Console.WriteLine($"Received register request from {regReq.username} at {regReq.timestamp}");
            var regRes = new RegisterResponse(RegisterResponse.State.OK, "");
            try
            {
                this.RegisterUser(regReq.username, clientState.endpoint.Address.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error registering user {regReq.username}: {e.Message}");
                regRes.responseState = RegisterResponse.State.ERROR;
            }
            var regResBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(regRes);
            Console.WriteLine($"Sending register response {regRes} to {clientState.endpoint}");
            var sentBytes = this.registerServer.socket.Send(regResBytes, regResBytes.Length, clientState.endpoint);
            Console.WriteLine($"Sent {sentBytes} bytes");

        }
        else
        {
            Console.WriteLine("Invalid AsyncState. Unable to receive messages.");
        }
    }
    public Task StartAsync()
    {
        Console.WriteLine("Starting server");
        InitializeDB();
        StartRetransmissionServer();
        StartRegisterServer();
        while (true)
        {
            this.retransmissionServer.socket.BeginReceive(new AsyncCallback(RetransmissionHandler), this.retransmissionServer);
            this.registerServer.socket.BeginReceive(new AsyncCallback(RegisterHandler), this.registerServer);
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        CentralRetransmissionServer server = new CentralRetransmissionServer();
        await server.StartAsync();
    }

}