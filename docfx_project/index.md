# UDP Multichat CLI Application

This chat app is a simple UDP client/server application that allows multiple clients
to connect to a server and send messages to each other. The server is able to handle
multiple clients at once and will broadcast messages to all connected clients.

## Basic functionality of a UDP Multichat

```mermaid
sequenceDiagram
    participant Client
    participant Server
    participant OtherRegisteredClients

    Client->>Server: Sends Authentication request
    Server->>Client: Sends Authentication response
    Client->>Server: Sends Message
    Server->>OtherRegisteredClients: Broadcasts Message
    Server->>Client: Sends Message Ack
```

## Possible client states

```mermaid
sequenceDiagram
    participant Client
    participant Registered
    participant NotRegistered
    participant LoggedIn
    participant NotLoggedIn
    participant SendingMessage
    participant ReceivingChatMessage

    Client ->> Registered: Can be
    Client ->> NotRegistered: Can be
    Registered ->> LoggedIn: Logs in
    NotLoggedIn ->> LoggedIn: Logs in
    NotRegistered ->> Registered: Register
    Registered ->> NotLoggedIn: Can be
    LoggedIn ->> NotLoggedIn: Log out
    LoggedIn ->> NotRegistered: Unregister
    LoggedIn ->> SendingMessage: Sends a message
    LoggedIn ->> ReceivingChatMessage: Receives a message retransmission from the server
```

## Possible server states

```mermaid
sequenceDiagram
    participant Server
    participant ListeningForMessages
    participant ListeningForAuthenticationRequests
    participant AnsweringAuthenticationRequests
    participant RetransmittingMessages

   
    Server ->> ListeningForMessages: Can be
    Server ->> ListeningForAuthenticationRequests: Can be
    ListeningForMessages ->> RetransmittingMessages: Receieves a message from a client
    ListeningForAuthenticationRequests ->>  AnsweringAuthenticationRequests: Receives an authentication request from a client 
```
