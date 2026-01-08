namespace SimpleDnsServer;

#nullable disable
public static class DnsConst
{
    public const string DNS_SERVER_PROCESS_NAME = "SimpleDnsServer";
    public const string FRAMEWORK = "net8.0";
    public const string DncControllerName = "dns";
    public const string DNS_ROOT = "/" + DncControllerName;
    //public const int UdpPort = 53;
    //public const int ApiPort = 60;

    public const int UdpPort = 10053;
    public const int ApiPort = 10060;

    // Try to increase UDP socket buffer size using reflection (ARSoft.Tools.Net does not expose Socket)
    public const int UDP_BUFFER = 8 * 1024 * 1024; //8MB

    public const string DNS_IP = "127.0.0.1";
    //public const string IP = "http://localhost";
    public const string DNS_IPv6 = "::1";

    private const string ipKey = "ip";
    private const string apiPortKey = "apiPort";
    private const string udpPortKey = "udpPort";
    private const string urlTemplate = "http://{0}:{1}/{2}";

    public static string URL = "http://" + DNS_IP + ":" + ApiPort;

    public static string ResolveUrl(IConfigurationRoot config)
    {
        return string.Format(urlTemplate, config[ipKey] ?? DNS_IP, config[apiPortKey] ?? ApiPort.ToString(), DncControllerName.ToLower());
    }

    public static string ResolveApiPort(IConfigurationRoot config) => config[apiPortKey] ?? ApiPort.ToString();

    public static string ResolveDnsIp(IConfiguration config) => config[ipKey] ?? DNS_IP;

    public static string ResolveUdpPort(IConfiguration config) => config[udpPortKey] ?? UdpPort.ToString();

    public static string ResolveUrl(string ip, int port)
    {
        return string.Format(urlTemplate, ip, port, DncControllerName.ToLower());
    }
}