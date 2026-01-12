namespace DnsClient;

public interface IHttpClient
{
    Task<HttpResponseMessage> GetAsync(string url);
    Task<HttpResponseMessage> PostAsync(string url, HttpContent? content = null);
}
