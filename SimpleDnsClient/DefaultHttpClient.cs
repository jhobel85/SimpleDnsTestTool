using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleDnsClient;

public class DefaultHttpClient : IHttpClient
{
    private static readonly HttpClient httpClient = new(new InsecureHttpClientHandler());

    public Task<HttpResponseMessage> GetAsync(string url)
        => httpClient.GetAsync(url);

    public Task<HttpResponseMessage> PostAsync(string url, HttpContent? content = null)
        => httpClient.PostAsync(url, content);
}
