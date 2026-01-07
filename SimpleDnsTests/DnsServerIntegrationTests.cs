using SimpleDnsClient;

namespace SimpleDnsServer.Tests
{
    public class DnsServerIntegrationTests(DnsServerFixture fixture) : IClassFixture<DnsServerFixture>
    {
        private const string TestDomain_V4 = "test.local";
        private const string TestIp_V4 = "192.168.1.100";

        private const string TestDomain_V6 = "test6.local";
        private const string TestIp_V6 = "fd00::100";

        private readonly DnsServerFixture _fixture = fixture;

       // [Fact]
        public void RegisterAndResolveDomain_ReturnsCorrectIPv4()
        {
            // Arrange: Register domain (assumes server is already running via fixture)            
            string dns_ip = Constants.DNS_IP;
            var dnsClient = new RestClient(dns_ip, Constants.ApiPort);            
            dnsClient.Register(TestDomain_V4, TestIp_V4);
            // Act: Send DNS query
            var resolvedIp = DnsTestUtils.SendDnsQueryIPv4(dns_ip, TestDomain_V4, Constants.UdpPort);

            // Assert
            Assert.Equal(TestIp_V4, resolvedIp);
        }

        [Fact]
        public void RegisterAndResolveDomain_ReturnsCorrectIPv6()
        {
            // Arrange: Register domain (assumes server is already running via fixture)
            string dns_ip = Constants.DNS_IPv6;
            var dnsClient = new RestClient(dns_ip, Constants.ApiPort, useIPv6: true);
            dnsClient.Register(TestDomain_V6, TestIp_V6);

            // Act: Send DNS query (AAAA record)
            var resolvedIp = DnsTestUtils.SendDnsQueryIPv6(dns_ip, TestDomain_V6, Constants.UdpPort);

            // Assert
            Assert.Equal(TestIp_V6.ToLowerInvariant(), resolvedIp.ToLowerInvariant());
        }        
    }
    
}
