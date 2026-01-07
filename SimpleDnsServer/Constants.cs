using Microsoft.Extensions.Configuration;

#nullable disable
public static class Constants
{
    public const string DNS_SERVER_PROCESS_NAME = "SimpleDnsServer";            
    public const string FRAMEWORK = "net8.0";
    public const string DncControllerName = "dns";
    public const string DNS_ROOT = "/" + DncControllerName;
    //public const int UdpPort = 53; // standard DNS port, but may be blocked by other DNS servers (Windows/Linux/MacOS)
    public const int UdpPort = 10053;
    public const int ApiPort = 60;

    public const string DNS_IP = "127.0.0.1";
    //public const string IP = "http://localhost";
    public const string DNS_IPv6 = "::1";

    private const string ipKey = "ip";
    private const string apiPortKey = "apiPort";
    private const string udpPortKey = "udpPort";
    private const string urlTemplate = "http://{0}:{1}/{2}";

    public static string ResolveUrl(IConfigurationRoot config)
    {
        return string.Format(urlTemplate, (object)(config[ipKey] ?? DNS_IP), (object)(config[apiPortKey] ?? ApiPort.ToString()), (object)DncControllerName.ToLower());
    }

    public static string ResolveApiPort(IConfigurationRoot config) => config[apiPortKey] ?? ApiPort.ToString();

    public static string ResolveDnsIp(IConfiguration config) => config[ipKey] ?? DNS_IP;

    public static string ResolveUdpPort(IConfiguration config) => config[udpPortKey] ?? UdpPort.ToString();

    public static string ResolveUrl(string ip, int port)
    {
        return string.Format(urlTemplate, (object)ip, (object)port, (object)DncControllerName.ToLower());
    }
}
