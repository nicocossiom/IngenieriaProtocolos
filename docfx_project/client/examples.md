# Examples of using the client

This section contains examples of using the client.

![Example of 8 users](../images/gif6users.gif)

## Register

```bash
Input a command, available commands are
            - register
            - login
            - send
            - unregister

register
Enter a username, spaces are allowed, but not empty string: (↩️  after input)
nico
Enter a password, spaces are allowed, but not empty string: (↩️  after input)
nico
Sending request Request(REGISTER,nico,nico,14/6/2023 17:41:57) to 127.0.0.1:5000...
Sent 93
Waiting for response from server 127.0.0.1:5000...
User registered successfully
Input a command, available commands are
            - register
            - login
            - send
            - unregister
```

## Login

```bash
Input a command, available commands are
            - register
            - login
            - send
            - unregister
login
Enter a username, spaces are allowed, but not empty string: (↩️  after input)
nico
Enter a password, spaces are allowed, but not empty string: (↩️  after input)
nico
Sending request Request(LOGIN,nico,nico,14/6/2023 17:42:50) to 127.0.0.1:5000...
Sent 93
Waiting for response from server 127.0.0.1:5000...
User logged in successfully
Currently logged in as nico
Input a command, available commands are
            - register
            - login
            - send
            - unregister
```

> Note: When a user logs in it's ipadress and port registered in the central server database are updated with the current ipadress and port.

## Send

Let's imagine we have two clients, one with username Nico and one with username Pedro.
Pedro sends a message to the server which is then sent to Nico.
Bob sends a message

```bash
Currently logged in as bob
Input a command, available commands are
            - register
            - login
            - send
            - unregister

send
Enter a message: (↩️  to send, shift +  ↩️  for new line)
Hola a todos
Waiting for response from server 127.0.0.1:5001...
Received response ChatMessageResponse(Received:True, RetransmittedNTimes:1)
Message was received correctly and retransmitted (with no garantees of reaching) to 1 clients
```

As seen in the response, the message was received correctly and retransmitted to 1 client.

```bash
Currently logged in as nico
Input a command, available commands are
            - register
            - login
            - send
            - unregister

Received message from server
Received message from bob: Hola a todos
```

> Note: The message was received correctly, but there is no garantee that it will reach any of the rest of the registered clients.
> The server sends the messages and does not wait for a response from the clients.

> WARNING: Users need to be logged in before being able to send messages. In order to log in, the user needs to be registered first.
> Users need to use the same retransmission server in order to be able to communicate.

## Unregister

> NOT IMPLEMENTED YET
