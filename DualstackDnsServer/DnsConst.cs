namespace DualstackDnsServer;

#nullable disable

public static class DnsConst
{
    public const string DNS_SERVER_PROCESS_NAME = "DualstackDnsServer";
    public const string FRAMEWORK = "net8.0";
    public const string DncControllerName = "dns";
    public const string DNS_ROOT = "/" + DncControllerName;
    public const int UdpPort = 53;
    public const int ApiHttp = 60;
    public const int ApiHttps = 44360;

    // Try to increase UDP socket buffer size using reflection (ARSoft.Tools.Net does not expose Socket)
    public const int UDP_BUFFER = 8 * 1024 * 1024; //8MB

    private const string ipKey = "ip";
    private const string ip6Key = "ip6";
    private const string apiPortKey = "apiPort";
    private const string udpPortKey = "udpPort";

    public const bool DEFAULT_ENABLE_HTTP = false; // HTTP enabled by default for tests

    /// <summary>
    /// Determines if HTTP endpoints should be enabled based on config/args. Default: disabled.
    /// Enable with --http, --http=true, or --http=1
    /// </summary>
    public static bool IsHttpEnabled(IConfiguration config, string[] args)
    {
        var httpValue = config["http"];
        if (!string.IsNullOrEmpty(httpValue))
        {
            if (bool.TryParse(httpValue, out var parsed))
                return parsed;
            if (string.Equals(httpValue, "1"))
                return true;
        }
        else if (args != null && args.Any(a => a.Equals("--http", StringComparison.OrdinalIgnoreCase)))
        {
            // Support for --http as a flag
            return true;
        }
        return DEFAULT_ENABLE_HTTP;
    }

    public static string GetDnsIp(DnsIpMode mode = DnsIpMode.Localhost, IConfiguration config = null)
    {
        return mode switch
        {
            DnsIpMode.Any => "0.0.0.0",
            DnsIpMode.Localhost => "127.0.0.1",
            DnsIpMode.Custom => config?[ipKey] ?? "127.0.0.1",
            _ => "127.0.0.1"
        };
    }

    public static string GetDnsIpV6(DnsIpMode mode = DnsIpMode.Localhost, IConfiguration config = null)
    {
        return mode switch
        {
            DnsIpMode.Any => "::",
            DnsIpMode.Localhost => "::1",
            DnsIpMode.Custom => config?[ip6Key] ?? "::1",
            _ => "::1"
        };
    }

    public static string ResolveDnsIp(IConfiguration config)
    {
        // Always use Custom mode when config is provided
        return GetDnsIp(DnsIpMode.Custom, config);
    }

    public static string ResolveDnsIpV6(IConfiguration config)
    {
        // Always use Custom mode when config is provided
        return GetDnsIpV6(DnsIpMode.Custom, config);
    }

    public static string ResolveApiPort(IConfigurationRoot config) => config[apiPortKey] ?? ApiHttp.ToString();
    public static string ResolveUdpPort(IConfiguration config) => config[udpPortKey] ?? UdpPort.ToString();

    public static string ResolveHttpUrl(IConfigurationRoot config)
    {
        string ipRes = ResolveDnsIp(config);
        return $"http://{ipRes}:{ApiHttp}";
    }

    public static string ResolveHttpsUrl(IConfigurationRoot config)
    {
        string ipRes = ResolveDnsIp(config);
        return $"https://{ipRes}:{ApiHttps}";
    }

    public static string ResolveHttpUrlV6(IConfigurationRoot config)
    {
        string ipRes = ResolveDnsIpV6(config);
        return $"http://[{ipRes}]:{ApiHttp}";
    }

    public static string ResolveHttpsUrlV6(IConfigurationRoot config)
    {
        string ipRes = ResolveDnsIpV6(config);
        return $"https://[{ipRes}]:{ApiHttps}";
    }
}