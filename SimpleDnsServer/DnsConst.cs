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

    public const string DNS_IP = "127.0.0.1"; //localhost
    public const string DNS_IP_ANY = "0.0.0.0";
    
    public const string DNS_IPv6 = "::1";//localhost
    public const string DNS_IPv6_ANY = "::";

    //private const string IP_OPTION = "localhost";
    //private const string IP_OPTION = "any";
    private const string ipKey = "ip"; // may be only "localhost" or "any"
    private const string apiPortKey = "apiPort";
    private const string udpPortKey = "udpPort";

    //public static string URL = "http://" + DNS_IP + ":" + ApiPort;

    public static string ResolveApiPort(IConfigurationRoot config) => config[apiPortKey] ?? ApiPort.ToString();

    public static string ResolveDnsIp(IConfiguration config) => config[ipKey] == "any" ? DNS_IP_ANY : DNS_IP;

    public static string ResolveDnsIpV6(IConfiguration config) => config[ipKey] == "any" ? DNS_IPv6_ANY: DNS_IPv6;

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