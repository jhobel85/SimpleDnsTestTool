using SimpleDnsServer;
using System.Net;

namespace SimpleDnsClient;


public class RestClient
{
    public Guid SessionId { get; } = Guid.NewGuid();
    private readonly string serverUrl;
    private readonly IHttpClient httpClient;

    public RestClient(string ip, int apiPort, IHttpClient? httpClient = null)
    {
        serverUrl = BuildUrl(ip, apiPort);
        // Ensure /dns is always present as the base path
        if (!serverUrl.EndsWith(DnsConst.DNS_ROOT, StringComparison.OrdinalIgnoreCase))
            serverUrl += DnsConst.DNS_ROOT;
        this.httpClient = httpClient ?? new DefaultHttpClient();
    }

    public async Task RegisterAsync(string domain, string ip, bool registerWithSessionContext)
    {
        if (registerWithSessionContext)
        {
            await RegisterAsync(domain, ip, SessionId.ToString());
        }
        else
        {
            await RegisterAsync(domain, ip);
        }
    }


    private async Task RegisterAsync(string domain, string ip, string sessionId)
    {
        var url = $"{serverUrl}/register/session?domain={domain}&ip={ip}&sessionId={sessionId}";
        var response = await httpClient.PostAsync(url, null);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error while registering domain. HttpStatusCode={response.StatusCode}");
        }
    }


    private async Task RegisterAsync(string domain, string ip)
    {
        var url = $"{serverUrl}/register?domain={domain}&ip={ip}";
        var response = await httpClient.PostAsync(url, null);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error while registering domain. HttpStatusCode={response.StatusCode}");
        }
    }


    public async Task<string> ResolveAsync(string domain)
    {
        var url = $"{serverUrl}/resolve?domain={domain}";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
        {
            Console.WriteLine($"Error while resolving domain. HttpStatusCode={response.StatusCode}");
        }
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
        {
            Console.WriteLine($"Error while unregistering domain. HttpStatusCode={response.StatusCode}");
        }
    }


    public async Task UregisterSessionAsync()
    {
        var url = $"{serverUrl}/unregister/session?sessionId={SessionId}";
        var response = await httpClient.PostAsync(url, null);
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
        {
            Console.WriteLine($"Error while unregistering domain. HttpStatusCode={response.StatusCode}");
        }
    }


    public async Task<int> SessionRecordsCountAsync()
    {
        var url = $"{serverUrl}/count/session?sessionId={SessionId}";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
        {
            Console.WriteLine($"Error while get SessionRecordsCount. HttpStatusCode={response.StatusCode}");
        }
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
        {
            Console.WriteLine($"Error while get RecordsCount. HttpStatusCode={response.StatusCode}");
        }
        string result = "";
        if (response.IsSuccessStatusCode)
            result = await response.Content.ReadAsStringAsync();
        return int.TryParse(result, out var count) ? count : 0;
    }

    private static string BuildUrl(string ip, int apiPort)
    {
        if (IPAddress.TryParse(ip, out var addr) && addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            return $"https://[{ip}]:{apiPort}/{DnsConst.DncControllerName}";
        return $"https://{ip}:{apiPort}/{DnsConst.DncControllerName}";
    }
}
