using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleDnsServer.Tests
{
    public static class DnsTestUtils
    {
        public static string SendDnsQueryIPv4(string dns_ip, string domain, int port)
        {
            using var client = new UdpClient();
            client.Connect(dns_ip, port);
            var query = BuildDnsQuery(domain);
            client.Send(query, query.Length);
            IPEndPoint remoteEP = new (IPAddress.Any, 0);
            var response = client.Receive(ref remoteEP);
            return ParseDnsResponseForARecord(response);
        }

        public static string SendDnsQueryIPv6(string dns_ip, string domain, int port)
        {
            using var client = new UdpClient(AddressFamily.InterNetworkV6);
            client.Connect(dns_ip, port);
            var query = BuildDnsQueryAAAA(domain);
            client.Send(query, query.Length);
            IPEndPoint remoteEP = new(IPAddress.IPv6Any, 0);
            var response = client.Receive(ref remoteEP);
            return ParseDnsResponseForAAAARecord(response);
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
}
