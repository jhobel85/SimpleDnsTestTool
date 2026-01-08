using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleDnsServer.Tests;

public static class ClientUtils
{
    public const int ClientTimeout = 5000;

    public static async Task<string> SendDnsQueryIPv4Async(string dns_ip, string domain, int port, CancellationToken cancellationToken = default)
    {
        using var client = new UdpClient();
        client.Connect(dns_ip, port);
        var query = BuildDnsQuery(domain);
        await client.SendAsync(query, query.Length);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(ClientTimeout);
        var receiveTask = client.ReceiveAsync(); // start the receive, then await its completion (or timeout) further.
        try
        {
            var completedTask = await Task.WhenAny(receiveTask, Task.Delay(Timeout.Infinite, cts.Token));
            if (completedTask == receiveTask)
            {
                var result = await receiveTask;
                return ParseDnsResponseForARecord(result.Buffer);
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

    public static async Task<string> SendDnsQueryIPv6Async(string dns_ip, string domain, int port, CancellationToken cancellationToken = default)
    {
        using var client = new UdpClient(AddressFamily.InterNetworkV6);
        client.Connect(dns_ip, port);
        var query = BuildDnsQueryAAAA(domain);
        await client.SendAsync(query, query.Length);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(ClientTimeout);
        var receiveTask = client.ReceiveAsync();
        try
        {
            var completedTask = await Task.WhenAny(receiveTask, Task.Delay(Timeout.Infinite, cts.Token));
            if (completedTask == receiveTask)
            {
                var result = await receiveTask;
                return ParseDnsResponseForAAAARecord(result.Buffer);
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

    public static byte[] BuildDnsQuery(string domain)
    {
        var rand = new Random();
        ushort id = (ushort)rand.Next(0, ushort.MaxValue);
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
        var qtype = new byte[] { 0x00, 0x01 };
        var qclass = new byte[] { 0x00, 0x01 };
        return [.. header, .. qname, .. qtype, .. qclass];
    }

    public static byte[] BuildDnsQueryAAAA(string domain)
    {
        var rand = new Random();
        ushort id = (ushort)rand.Next(0, ushort.MaxValue);
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
        var qtype = new byte[] { 0x00, 0x1c };
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
