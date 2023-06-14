namespace UdpChat.Server.CLI
{
    /// <inheritdoc/>
    public class ServerCLI
    {

        private static void PrintHelp()
        {
            Console.WriteLine(@"
UdpChat.Server: Starts the server
    Usage: UdpChat.Server [OPTIONS]
    Options:
        -h, --help -  Prints this help message
        -p, --port - Port to listen on. Defaults to 5000 for Auth requests and port+1 for Message requests");
        }
        /// <summary>
        /// Parses the command line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int? ParseArgs(string[] args)
        {
            if (args.Length == 0)
            {
                return null;
            }
            else if (args.Length == 1)
            {
                if (args[0] == "-h" || args[0] == "--help")
                {
                    PrintHelp();
                    System.Environment.Exit(0);
                    return null;
                }
                else
                {
                    Console.WriteLine("Invalid argument");
                    PrintHelp();
                    System.Environment.Exit(0);
                    return null;
                }
            }
            else if (args.Length == 2)
            {
                if (args[0] == "-p" || args[0] == "--port")
                {
                    if (int.TryParse(args[1], out int port))
                    {
                        return port;
                    }
                    else
                    {
                        Console.WriteLine("Invalid port");
                        PrintHelp();
                        System.Environment.Exit(0);
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid argument");
                    PrintHelp();
                    System.Environment.Exit(0);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("Too many arguments");
                PrintHelp();
                System.Environment.Exit(0);
                return null;
            }
        }
    }
}