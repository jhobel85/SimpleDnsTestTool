using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DualstackDnsServer.Services;

public class DnsUdpClientService : IDnsUdpClientService
{
    private readonly ServerOptions _serverOptions;
    public const int ClientTimeout = 5000;

    public DnsUdpClientService(ServerOptions serverOptions)
    {
        _serverOptions = serverOptions;
    }
        
    public async Task<string> QueryDnsAsync(string domain, CancellationToken cancellationToken = default)
    {
        // Try IPv6 (AAAA) first
        string ipv6 = await QueryDnsAsync(_serverOptions.IpV6, domain, _serverOptions.UdpPort, QueryType.AAAA, cancellationToken);
        if (!string.IsNullOrWhiteSpace(ipv6))
            return ipv6;

        // Then try IPv4 (A)
        string ipv4 = await QueryDnsAsync(_serverOptions.Ip, domain, _serverOptions.UdpPort, QueryType.A, cancellationToken);
        if (!string.IsNullOrWhiteSpace(ipv4))
            return ipv4;

        return string.Empty;
    }

    public async Task<string> QueryDnsAsync(string dnsServer, string domain, int port, QueryType type, CancellationToken cancellationToken = default)
    {
        AddressFamily family = type == QueryType.A ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
        using var client = new UdpClient(family);
        client.Connect(dnsServer, port);
        var query = BuildDnsQueryGeneric(domain, type);
        await client.SendAsync(query, query.Length);
        var receiveTask = client.ReceiveAsync(cancellationToken).AsTask();
        try
        {
            var completedTask = await Task.WhenAny(receiveTask, Task.Delay(ClientTimeout, cancellationToken));
            if (completedTask == receiveTask)
            {
                var result = await receiveTask;
                return type == QueryType.A ? ParseDnsResponseForARecord(result.Buffer) : ParseDnsResponseForAAAARecord(result.Buffer);
            }
            else
            {
                throw new TimeoutException($"DNS query for {domain} timed out.");
            }
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"DNS query for {domain} timed out.");
        }
    }

    public Task<string> QueryDnsIPv4Async(string dns_ip, string domain, int port, CancellationToken cancellationToken = default)
        => QueryDnsAsync(dns_ip, domain, port, QueryType.A, cancellationToken);

    public Task<string> QueryDnsIPv6Async(string dns_ip, string domain, int port, CancellationToken cancellationToken = default)
        => QueryDnsAsync(dns_ip, domain, port, QueryType.AAAA, cancellationToken);

    public static byte[] BuildDnsQueryGeneric(string domain, QueryType type)
    {
        ushort id = (ushort)System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, ushort.MaxValue + 1);
        var header = new byte[] {
                (byte)(id >> 8), (byte)(id & 0xFF),
                0x01, 0x00,
                0x00, 0x01,
                0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00
            };
        var qname = new List<byte>();
        foreach (var part in domain.Split('.'))
        {
            qname.Add((byte)part.Length);
            qname.AddRange(Encoding.ASCII.GetBytes(part));
        }
        qname.Add(0);
        var qtype = type == QueryType.A ? new byte[] { 0x00, 0x01 } : new byte[] { 0x00, 0x1c };
        var qclass = new byte[] { 0x00, 0x01 };
        return [.. header, .. qname, .. qtype, .. qclass];
    }

    public static string ParseDnsResponseForARecord(byte[] response)
    {
        return ParseAnswer(response, 1, 4, bytes => string.Join(".", bytes));
    }
    public static string ParseDnsResponseForAAAARecord(byte[] response)
    {
        return ParseAnswer(response, 28, 16, bytes => new IPAddress(bytes.ToArray()).ToString());
    }
    
    private static string ParseAnswer(byte[] response, int desiredType, int expectedLength, Func<IEnumerable<byte>, string> projector)
    {
        if (response.Length < 12) return string.Empty;
        int anCount = (response[6] << 8) | response[7];
        int ptr = 12;
        // Skip QNAME
        while (ptr < response.Length && response[ptr] != 0) ptr += response[ptr] + 1;
        if (ptr >= response.Length) return string.Empty;
        ptr++; // null label
        ptr += 4; // QTYPE + QCLASS
        for (int i = 0; i < anCount; i++)
        {
            if (ptr >= response.Length) return string.Empty;
            // NAME
            if ((response[ptr] & 0xC0) == 0xC0)
            {
                ptr += 2;
            }
            else
            {
                while (ptr < response.Length && response[ptr] != 0) ptr += response[ptr] + 1;
                ptr++;
            }
            if (ptr + 10 > response.Length) return string.Empty;
            ushort type = (ushort)(response[ptr] << 8 | response[ptr + 1]);
            ptr += 2; // TYPE
            ptr += 2; // CLASS
            ptr += 4; // TTL
            ushort rdlength = (ushort)(response[ptr] << 8 | response[ptr + 1]);
            ptr += 2;
            if (type == desiredType && rdlength == expectedLength && ptr + expectedLength <= response.Length)
            {
                return projector(response.Skip(ptr).Take(expectedLength));
            }
            ptr += rdlength;
        }
        return string.Empty;
    }
}
