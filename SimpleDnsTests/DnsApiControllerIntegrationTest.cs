using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleDnsServer;
using Xunit;

namespace SimpleDnsTests
{
    public class DnsApiControllerIntegrationTest : IClassFixture<WebApplicationFactory<SimpleDnsServer.Startup>>
    {
        private readonly HttpClient _client;

        public DnsApiControllerIntegrationTest(WebApplicationFactory<SimpleDnsServer.Startup> factory)
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
    }
}
