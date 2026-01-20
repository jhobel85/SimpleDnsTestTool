using System.Net;

namespace DualstackDnsServer;

public static class CliArgumentValidator
{
    public static ServerOptions ParseServerOptions(IConfiguration config, string[] args)
    {
        // Use DnsConst for all key names
        string ip = ReadIp(DnsConst.ipKey, config, GetDnsIp(), allowEmpty: false);
        string ipV6 = ReadIp(DnsConst.ip6Key, config, string.Empty, allowEmpty: true);
        int httpsPortValue = ReadPort(DnsConst.portHttpsKey, config, DnsConst.PortHttps);
        int httpPortValue = ReadPort(DnsConst.portHttpKey, config, DnsConst.PortHttp);
        int udpPortValue = ReadPort(DnsConst.portUdpKey, config, DnsConst.PortUdp);

        return new ServerOptions
        {
            Ip = ip,
            IpV6 = ipV6,
            HttpsPort = httpsPortValue,
            HttpPort = httpPortValue,
            UdpPort = udpPortValue,
            CertPath = GetCertPath(config),
            CertPassword = GetCertPassword(config),
            EnableHttp = IsHttpEnabled(config, args),
            IsVerbose = IsVerbose(config),
            Args = args
        };
    }

    public static string ReadIp(string key, IConfiguration config, string fallback, bool allowEmpty)
    {
        var raw = config[key];
        if (string.IsNullOrWhiteSpace(raw))
            return allowEmpty ? string.Empty : fallback;
        if (IPAddress.TryParse(raw, out _))
            return raw;
        Console.WriteLine($"[WARN] Invalid {key} address '{raw}', {(allowEmpty ? "disabling" : "falling back to " + fallback)}");
        return allowEmpty ? string.Empty : fallback;
    }

    public static int ReadPort(string key, IConfiguration config, int fallback)
    {
        var raw = config[key];
        if (string.IsNullOrWhiteSpace(raw))
            return fallback;
        if (int.TryParse(raw, out var val))
            return val;
        Console.WriteLine($"[WARN] Invalid {key} '{raw}', falling back to {fallback}");
        return fallback;
    }


    public static bool ValidateArgs(string[] args, HashSet<string> supportedKeys)
    {
        bool printedHelp = false;
        foreach (var arg in args)
        {
            if (string.Equals(arg, "--h", StringComparison.OrdinalIgnoreCase) || string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase))
            {
                PrintHelp();
                Environment.Exit(0);
            }
        }
        foreach (var arg in args)
        {
            // If any argument starts with a single dash but not double dash, print help and exit
            if (arg.StartsWith("-") && !arg.StartsWith("--"))
            {
                Console.WriteLine($"[ERROR] Invalid argument '{arg}'. All arguments must use double dashes (e.g., --ip, --portHttps).\n");
                PrintHelp();
                Environment.Exit(1);
            }
        }
        foreach (var arg in args)
        {
            if (!arg.StartsWith("--")) continue;
            var trimmed = arg.TrimStart('-');
            var key = trimmed.Split('=', 2, StringSplitOptions.RemoveEmptyEntries)[0];
            if (string.IsNullOrWhiteSpace(key)) continue;
            if (!supportedKeys.Contains(key))
            {
                Console.WriteLine($"[WARN] Unsupported parameter '{key}' will be ignored.");
                if (!printedHelp)
                {
                    PrintHelp();
                    printedHelp = true;
                }
            }
        }
        return printedHelp;
    }

    public static void PrintHelp()
    {
        Console.WriteLine("\nUsage: DualstackDnsServer [--ip <IPv4>] [--ip6 <IPv6>] [--portHttp <port>] [--portHttps <port>] [--portUdp <port>] [--http] [--cert <path>] [--certPassw <password>] [--v|--verbose]\n");
        Console.WriteLine("  --ip           Bind IPv4 address (default: 127.0.0.1)");
        Console.WriteLine("  --ip6          Bind IPv6 address (default: disabled)");
        Console.WriteLine("  --portHttp     HTTP port (default: 80, only if --http)");
        Console.WriteLine("  --portHttps    HTTPS port (default: 443)");
        Console.WriteLine("  --portUdp      UDP DNS port (default: 53)");
        Console.WriteLine("  --http         Enable HTTP endpoint (default: disabled)");
        Console.WriteLine("  --cert         Path to HTTPS certificate");
        Console.WriteLine("  --certPassw    Password for HTTPS certificate");
        Console.WriteLine("  --v, --verbose Enable verbose logging");
    }

        public static bool IsVerbose(IConfiguration config)
    {
        foreach (var key in DnsConst.verboseKeys)
        {
            var value = config[key];
            if (value != null)
            {
                // If --v or --verbose is present with no value, treat as false (default)
                if (string.IsNullOrEmpty(value))
                    return false;
                if (bool.TryParse(value, out var parsed))
                    return parsed;
                if (string.Equals(value, "1"))
                    return true;
            }
        }
        return false;
    }

    public static string GetCertPath(IConfiguration config) => config?[DnsConst.certPathKey] ?? string.Empty;

    public static string GetCertPassword(IConfiguration config) => config?[DnsConst.certPasswordKey] ?? string.Empty;

    public static bool IsHttpEnabled(IConfiguration config, string[] args)
    {
        var httpValue = config[DnsConst.httpEnabledKey];
        if (httpValue != null)
        {
            // If --http is present with no value, treat as true
            if (string.IsNullOrEmpty(httpValue))
                return true;
            if (bool.TryParse(httpValue, out var parsed))
                return parsed;
            if (string.Equals(httpValue, "1"))
                return true;
        }
        return DnsConst.DEFAULT_ENABLE_HTTP;
    }

    public static string GetDnsIp(DnsIpMode mode = DnsIpMode.Localhost, IConfiguration? config = null)
    {
        return mode switch
        {
            DnsIpMode.Any => "0.0.0.0",
            DnsIpMode.Localhost => "127.0.0.1",
            DnsIpMode.Custom => config?[DnsConst.ipKey] ?? "127.0.0.1",
            _ => "127.0.0.1"
        };
    }

    public static string GetDnsIpV6(DnsIpMode mode = DnsIpMode.Localhost, IConfiguration? config = null)
    {
        return mode switch
        {
            DnsIpMode.Any => "::",
            DnsIpMode.Localhost => "::1",
            // Default to disabled (empty) unless explicitly provided via --ip6
            DnsIpMode.Custom => config?[DnsConst.ip6Key] ?? string.Empty,
            _ => string.Empty
        };
    }

    public static string GetDnsHostname(DnsIpMode mode = DnsIpMode.Localhost)
    {
        return mode switch
        {
            DnsIpMode.Any => "localhost",
            DnsIpMode.Localhost => "localhost",
            DnsIpMode.Custom => "mydns.local", // currently only statically, not possible to define via config
            _ => "localhost"
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

    public static string ResolveHttpsPort(IConfigurationRoot config)
    {
        // Prefer explicit HTTPS port, otherwise default 443
        return config[DnsConst.portHttpsKey]
            ?? DnsConst.PortHttps.ToString();
    }

    public static string ResolveHttpPort(IConfiguration config)
    {
        return config[DnsConst.portHttpKey] ?? DnsConst.PortHttp.ToString();
    }
    public static string ResolveUdpPort(IConfiguration config) => config[DnsConst.portUdpKey] ?? DnsConst.PortUdp.ToString();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4423", Justification = "HTTP is used only for local development and testing; HTTPS is enforced in production.")]
    public static string ResolveHttpUrl(IConfigurationRoot config)
    {
        string ipRes = ResolveDnsIp(config);
        return $"http://{ipRes}:{DnsConst.PortHttp}";
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4423", Justification = "HTTP is used only for local development and testing; HTTPS is enforced in production.")]
    public static string ResolveHttpUrlV6(IConfigurationRoot config)
    {
        string ipRes = ResolveDnsIpV6(config);
        return $"http://[{ipRes}]:{DnsConst.PortHttp}";
    }

    // Deprecated: Use dynamic port in Program.cs
    public static string ResolveHttpsUrlV6(IConfigurationRoot config)
    {
        string ipRes = ResolveDnsIpV6(config);
        return $"https://[{ipRes}]:{DnsConst.PortHttps}";
    }
    
}