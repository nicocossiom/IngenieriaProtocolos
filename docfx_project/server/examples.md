# Examples of using the server

This section contains examples of using the server.

## Server output

The server will output information about what it is doing, for example when a user registers or logs in.
Errors will also be outputted into STDERR.

```bash
dotnet run --project UdpChat.Server/
Creating server with ports 5000 and 5001
Starting server
Opening server database
Users table created with 0 changes
Retransmission server started at 127.0.0.1:5001
Authentication server started at 127.0.0.1:5000
Received request from 127.0.0.1:4000:
 Request(REGISTER,nico,nico,14/6/2023 17:41:57)
Register SQL operation INSERT INTO users (username, password, ip_address, port) VALUES (@username, @password, @ip_address, @port) Microsoft.Data.Sqlite.SqliteParameterCollection
 Sending register response RegisterResponse: REGISTER_SUCCESS to 127.0.0.1:4000
 Sent 107 bytes
Received request from 127.0.0.1:4000:
 Request(LOGIN,nico,nico,14/6/2023 17:42:50)
Received login request from nico at 14/6/2023 17:42:50
User nico updated address and port
 Sent 105 bytes
Received request from 127.0.0.1:4004:
 Request(REGISTER,bob,bob,14/6/2023 17:45:41)
Register SQL operation INSERT INTO users (username, password, ip_address, port) VALUES (@username, @password, @ip_address, @port) Microsoft.Data.Sqlite.SqliteParameterCollection
 Sending register response RegisterResponse: REGISTER_SUCCESS to 127.0.0.1:4004
 Sent 107 bytes
Received request from 127.0.0.1:4004:
 Request(LOGIN,bob,bob,14/6/2023 17:45:47)
Received login request from bob at 14/6/2023 17:45:47
User bob updated address and port
 Sent 106 bytes
Received message from bob@127.0.0.1:4004 at 14/6/2023 17:45:53:
 Hola a todos
Retransmitting message to all 1 registered users
 Retransmitting message to nico@127.0.0.1:4001
 Sent 116 bytes to 127.0.0.1:4001
Sending response to bob@127.0.0.1:4004
Sent 88 bytes to 127.0.0.1:4004
^CStopping server and related services
Closing database connection
Exited successfully
```

## Error logging

Logging errors can be achieved by redirecting STDERR to a file.

```bash
dotnet run --project UdpChat.Server/ 2> errorfile.txt
```
