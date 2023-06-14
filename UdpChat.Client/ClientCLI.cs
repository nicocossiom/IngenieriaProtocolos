using UdpChat.Lib.Authentication;

namespace UdpChat.Client.CLI
{
    /// <summary>
    /// The ClientCLI class is a command line interface for the client. 
    /// It is used to create a <see cref="ChatClient"/> instance with default or custom settings. 
    /// See <see href="/client">Client CLI Documentation</see> for more information.
    /// </summary>
    public class ClientCLI
    {
        /// <summary> Returns a ChatClient with a custom configuration from user input </summary>
        /// <remarks> When called this method will ask the user for a port for the client, an IP for the central server and a port for the central server </remarks>
        /// <returns> A ChatClient instance, either with the specified settings or custom settings</returns> 
        private static ChatClient ChatClientFromInputCustomConfig()
        {
            Console.WriteLine("Enter a port for the client (enter for 4000):");
            var port = Console.ReadLine();
            port = string.IsNullOrEmpty(port) ? "4000" : port;

            Console.WriteLine("IP of central server (enter for 127.0.0.1):");
            var centralServerAddress = Console.ReadLine();
            centralServerAddress = string.IsNullOrEmpty(centralServerAddress) ? "127.0.0.1" : centralServerAddress;

            Console.WriteLine("Port of central server (enter for 5000):");
            var centralServerPort = Console.ReadLine();
            centralServerPort = string.IsNullOrEmpty(centralServerPort) ? "5000" : centralServerPort;


            if (!int.TryParse(port, out int portInt) || !int.TryParse(centralServerPort, out int centralServerPortInt))
            {
                Console.Error.WriteLine("Invalid ports, retrying...");
                return ChatClientFromInputCustomConfig();
            }
            string settingsString = $@"
    Client config:
        Client ports: Receive={portInt} - Send={portInt + 1}
        Central Server: Address={centralServerAddress} - Port={centralServerPortInt}
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
            return new ChatClient(portInt, centralServerAddress, centralServerPortInt);
        }
        /// <summary>
        /// Asks the user if they want to use the default settings or a custom configuration
        /// </summary>
        /// <remarks>
        /// Default settings:
        /// Client ports: Receive 4000 - Send 4001
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
        public static Task Main(string[] args)
        {
            Console.WriteLine("Welcome to the UDP chat client!");
            ChatClient client = ChatClientFromInput();
            if (!client.PingCentralServer())
            {
                Console.WriteLine("Could not reach central server. The server is no online. Exiting...");
                return Task.CompletedTask;
            }
            // start receiving messages
            client.StartMessageService();

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