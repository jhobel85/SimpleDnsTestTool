using System.Net;

public static class DnsBindModeHelper
{
    public static IPAddress GetBindAddress(DnsBindMode mode)
    {
        return mode switch
        {
            DnsBindMode.Loopback => IPAddress.IPv6Loopback, // dual-stack on Windows
            DnsBindMode.Any => IPAddress.IPv6Any,           // dual-stack on Windows
            DnsBindMode.IPv4Only => IPAddress.Loopback,
            DnsBindMode.IPv6Only => IPAddress.IPv6Loopback,
            _ => IPAddress.IPv6Loopback
        };
    }

    public enum DnsBindMode
    {
        Loopback,   // ::1 (dual-stack localhost)
        Any,        // [::] (dual-stack all interfaces)
        IPv4Only,   // 127.0.0.1
        IPv6Only    // ::1 (IPv6 only)
    }
}
