namespace DualstackDnsServer;

#nullable disable

public static class DnsConst

{
    public const string DNS_SERVER_PROCESS_NAME = "DualstackDnsServer";
    public const string FRAMEWORK = "net8.0";
    public const string DncControllerName = "dns";
    public const string DNS_ROOT = "/" + DncControllerName;

    public const bool DEFAULT_ENABLE_HTTP = false;
    
    public const int PortUdp = 53; // use non-privileged port to avoid conflicts with system DNS
    public const int PortHttp = 80;
    public const int PortHttps = 443;

    // Try to increase UDP socket buffer size using reflection (ARSoft.Tools.Net does not expose Socket)
    public const int UDP_BUFFER = 8 * 1024 * 1024; //8MB

    public const string ipKey = "ip";
    public const string ip6Key = "ip6";

    public const string httpEnabledKey = "http";

    public const string portHttpKey = "portHttp";
    public const string portHttpsKey = "portHttps";    
    public const string portUdpKey = "portUdp";
    public const string certPathKey = "cert";
    public const string certPasswordKey = "certPassw";    

    public static readonly string[] verboseKeys = ["v", "verbose"];    

    private static HashSet<string> supportedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ip6Key,
        ipKey,
        httpEnabledKey,
        portHttpKey,
        portHttpsKey,
        portUdpKey,
        certPathKey,
        certPasswordKey,
        verboseKeys[0],//v
        verboseKeys[1]//verbose
    };

    public static HashSet<string> SupportedKeys { get => supportedKeys; set => supportedKeys = value; }

}