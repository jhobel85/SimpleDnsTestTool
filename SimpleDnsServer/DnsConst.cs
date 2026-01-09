namespace SimpleDnsServer;

#nullable disable
public static class DnsConst
{
    public const string DNS_SERVER_PROCESS_NAME = "SimpleDnsServer";
    public const string FRAMEWORK = "net8.0";
    public const string DncControllerName = "dns";
    public const string DNS_ROOT = "/" + DncControllerName;
    public const int UdpPort = 53;
    public const int ApiPort = 60;

    // Try to increase UDP socket buffer size using reflection (ARSoft.Tools.Net does not expose Socket)
    public const int UDP_BUFFER = 8 * 1024 * 1024; //8MB

    //public const string DNS_IP = "0.0.0.0";   //any
    public const string DNS_IP = "127.0.0.1"; //localhost        
    //public const string DNS_IP = "192.168.50.1"; //example local network IP
            
    //public const string DNS_IPv6 = "::"; //any
    public const string DNS_IPv6 = "::1";//localhost
    //public const string DNS_IPv6 = "fd00:50::1"; //example local network IP       

    private const string ipKey = "ip"; // may be only "localhost" or "any"
    private const string ip6Key = "ip6";
    
    private const string apiPortKey = "apiPort";
    private const string udpPortKey = "udpPort";

    public static string ResolveDnsIp(IConfiguration config) => config[ipKey] ?? DNS_IP;

    public static string ResolveDnsIpV6(IConfiguration config) => config[ip6Key] ?? DNS_IPv6;

    public static string ResolveApiPort(IConfigurationRoot config) => config[apiPortKey] ?? ApiPort.ToString();
    public static string ResolveUdpPort(IConfiguration config) => config[udpPortKey] ?? UdpPort.ToString();

    public static string ResolveUrl(IConfigurationRoot config)
    {
        string ipRes = ResolveDnsIp(config);
        string port = config[apiPortKey] ?? ApiPort.ToString();
        return $"http://{ipRes}:{port}";
    }

    public static string ResolveUrlV6(IConfigurationRoot config)
    {
        string ipRes = ResolveDnsIpV6(config);
        string port = config[apiPortKey] ?? ApiPort.ToString();
        return $"http://[{ipRes}]:{port}";
    }
}