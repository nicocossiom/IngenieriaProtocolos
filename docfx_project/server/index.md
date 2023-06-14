
# Server CLI Documentation

This CLI launches a server that can be used for the UDPChat client.

## Usage

Available argunents are:

- -p or --port: The port the server will listen on. Default is 5000.

> NOTE: The server uses two ports, so the specified port and port + 1 will be used.

To exit the server in a controlled manner press `Ctrl + C`. This will ensure the server closes its
opened resources in an orderly fashion.

### Default settings

```bash
dotnet run --project UdpChat.Server/
Creating server with ports 5000 and 5001
Starting server
Opening server database
Users table created with 0 changes
Retransmission server started at 127.0.0.1:5001
Authentication server started at 127.0.0.1:5000
```

### Custom settings

```bash
dotnet run --port 6000 --project UdpChat.Server/
Creating server with ports 6000 and 6001
Starting server
Opening server database
Users table created with 0 changes
Retransmission server started at 127.0.0.1:6001
Authentication server started at 127.0.0.1:6000
```

> NOTE: If using the dotnet cli to run the server, if the server is run with the --project flag argyments
> for the server must be right after `dotnet run [server_args] --project <project> [dotnet_args]`.
