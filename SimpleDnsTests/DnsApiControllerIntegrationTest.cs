using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleDnsServer;
using Xunit;

namespace SimpleDnsTests
{
    public class DnsApiControllerIntegrationTest : IClassFixture<WebApplicationFactory<SimpleDnsServer.Program>>
    {
        private readonly HttpClient _client;

        public DnsApiControllerIntegrationTest(WebApplicationFactory<SimpleDnsServer.Program> factory)
        {
            _client = factory.CreateClient(); //default dynamic port assignment
            
            //fixed IP/port assignment
            /* 
            var customFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseUrls(DnsConst.URL);
            });
            _client = customFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri(DnsConst.URL)
            });
            */
        }

        [Fact]
        public async Task Resolve_Endpoint_ReturnsOk()
        {
            // Arrange: Register a domain first (if needed, depending on your API logic)
            var registerResponse = await _client.PostAsync("/dns/register?domain=example.com&ip=1.2.3.4", null);
            registerResponse.EnsureSuccessStatusCode();

            // Act: Call the resolve endpoint
            var response = await _client.GetAsync("/dns/resolve?domain=example.com");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("1.2.3.4", content);
        }

        [Fact]
        public async Task GetAllEntries_ReturnsBothIPv4AndIPv6()
        {
            // Arrange: Register IPv4 and IPv6 records
            var registerV4 = await _client.PostAsync("/dns/register?domain=example.com&ip=1.2.3.4", null);
            registerV4.EnsureSuccessStatusCode();
            var registerV6 = await _client.PostAsync("/dns/register?domain=ipv6.com&ip=2001:db8::1", null);
            registerV6.EnsureSuccessStatusCode();

            // Act: Call the GetAllEntries endpoint
            var response = await _client.GetAsync("/dns/entries");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("1.2.3.4", content); // IPv4
            Assert.Contains("2001:db8::1", content); // IPv6
            Assert.Contains("example.com", content);
            Assert.Contains("ipv6.com", content);
        }
    }
}
