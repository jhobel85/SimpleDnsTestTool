using Xunit;
using System.Threading.Tasks;
using DnsClient;

namespace DnsTests
{
    public class DefaultHttpClientTest
    {
        [Fact]
        public async Task GetAsync_ReturnsResponse()
        {
            var client = new DefaultHttpClient();
            var response = await client.GetAsync("https://httpbin.org/get");
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}
