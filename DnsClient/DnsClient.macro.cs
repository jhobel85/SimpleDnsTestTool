// DnsClient.macro.cs
// All client functionality merged for macro use
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DnsClientMacro
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent? content = null);
    }

    public class InsecureHttpClientHandler : HttpClientHandler
    {
        public InsecureHttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
    }

    public class DefaultHttpClient : IHttpClient
    {
        private static readonly HttpClient httpClient = new(new InsecureHttpClientHandler());

        public Task<HttpResponseMessage> GetAsync(string url)
            => httpClient.GetAsync(url);

        public Task<HttpResponseMessage> PostAsync(string url, HttpContent? content = null)
            => httpClient.PostAsync(url, content);
    }

    public class RestClient
    {
        public Guid SessionId { get; } = Guid.NewGuid();
        private readonly string serverUrl;
        private readonly IHttpClient httpClient;

        /// <summary>
        /// Create a new RestClient.
        /// </summary>
        /// <param name="ip">Server IP address</param>
        /// <param name="apiPort">API port</param>
        /// <param name="protocol">"http" or "https" (default: "https")</param>
        /// <param name="httpClient">Optional custom IHttpClient</param>
        public RestClient(string ip, int apiPort, string protocol = "http", IHttpClient? httpClient = null)
        {
            serverUrl = BuildUrl(ip, apiPort, protocol);
            if (!serverUrl.EndsWith("/dns", StringComparison.OrdinalIgnoreCase))
                serverUrl += "/dns";
            this.httpClient = httpClient ?? new DefaultHttpClient();
        }

        public async Task RegisterAsync(string domain, string ip, bool registerWithSessionContext)
        {
            if (registerWithSessionContext)
                await RegisterAsync(domain, ip, SessionId.ToString());
            else
                await RegisterAsync(domain, ip);
        }

        private async Task RegisterAsync(string domain, string ip, string sessionId)
        {
            var url = $"{serverUrl}/register/session?domain={domain}&ip={ip}&sessionId={sessionId}";
            var response = await httpClient.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Error while registering domain. HttpStatusCode={response.StatusCode}");
        }

        private async Task RegisterAsync(string domain, string ip)
        {
            var url = $"{serverUrl}/register?domain={domain}&ip={ip}";
            var response = await httpClient.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Error while registering domain. HttpStatusCode={response.StatusCode}");
        }

        public async Task<string> ResolveAsync(string domain)
        {
            var url = $"{serverUrl}/resolve?domain={domain}";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                Console.WriteLine($"Error while resolving domain. HttpStatusCode={response.StatusCode}");
            string result = "";
            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public async Task UnregisterAsync(string domain)
        {
            var url = $"{serverUrl}/unregister?domain={domain}";
            var response = await httpClient.PostAsync(url, null);
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                Console.WriteLine($"Error while unregistering domain. HttpStatusCode={response.StatusCode}");
        }

        public async Task UregisterSessionAsync()
        {
            var url = $"{serverUrl}/unregister/session?sessionId={SessionId}";
            var response = await httpClient.PostAsync(url, null);
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                Console.WriteLine($"Error while unregistering domain. HttpStatusCode={response.StatusCode}");
        }

        public async Task<int> SessionRecordsCountAsync()
        {
            var url = $"{serverUrl}/count/session?sessionId={SessionId}";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                Console.WriteLine($"Error while get SessionRecordsCount. HttpStatusCode={response.StatusCode}");
            string result = "";
            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadAsStringAsync();
            return int.TryParse(result, out var count) ? count : 0;
        }

        public async Task<int> RecordsCountAsync()
        {
            var url = $"{serverUrl}/count";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Error while get RecordsCount. HttpStatusCode={response.StatusCode}");
            string result = "";
            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadAsStringAsync();
            return int.TryParse(result, out var count) ? count : 0;
        }

        private static string BuildUrl(string ip, int apiPort, string protocol)
        {
            protocol = protocol?.ToLowerInvariant() == "http" ? "http" : "https";
            if (IPAddress.TryParse(ip, out var addr) && addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return $"{protocol}://[{ip}]:{apiPort}/dns";
            return $"{protocol}://{ip}:{apiPort}/dns";
        }
    }

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
            var qname = new System.Collections.Generic.List<byte>();
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
}
