namespace UdpChat
{
    [Serializable]
    public class EndPoint
    {
        public string ip { get; set; }
        public int port { get; set; }
        public EndPoint(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public override string? ToString()
        {
            return $"{ip}:{port}";
        }
    }
}