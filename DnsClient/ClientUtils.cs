using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DualstackDnsServer.Tests;

public static class ClientUtils
{
    public const int ClientTimeout = 5000;


    public static async Task<string> SendDnsQueryAsync(string dns_ip, string domain, int port, AddressFamily family, QueryType type, CancellationToken cancellationToken = default)
    {
        using var client = new UdpClient(family);
        client.Connect(dns_ip, port);
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

    public static Task<string> SendDnsQueryIPv4Async(string dns_ip, string domain, int port, CancellationToken cancellationToken = default)
        => SendDnsQueryAsync(dns_ip, domain, port, AddressFamily.InterNetwork, QueryType.A, cancellationToken);

    public static Task<string> SendDnsQueryIPv6Async(string dns_ip, string domain, int port, CancellationToken cancellationToken = default)
        => SendDnsQueryAsync(dns_ip, domain, port, AddressFamily.InterNetworkV6, QueryType.AAAA, cancellationToken);


    public enum QueryType { A, AAAA }

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
        int answerStart = response.Length - 4;
        return string.Join(".", response.Skip(answerStart).Take(4));
    }

    public static string ParseDnsResponseForAAAARecord(byte[] response)
    {
        int answerStart = response.Length - 16;
        var bytes = response.Skip(answerStart).Take(16).ToArray();
        return new IPAddress(bytes).ToString();
    }
}
