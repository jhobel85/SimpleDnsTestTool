using Xunit;
using DnsClient;

namespace SimpleDnsTests
{    
    public class RestClientTest
    {
        [Fact]
        public void CanCreateRestClient()
        {
            var client = new RestClient("127.0.0.1", 5000);
            Assert.NotNull(client);
        }
    }

}
