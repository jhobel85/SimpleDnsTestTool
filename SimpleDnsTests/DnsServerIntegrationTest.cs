using SimpleDnsClient;

namespace SimpleDnsServer.Tests
{
    public class DnsServerIntegrationTest(DnsServerFixture fixture) : IClassFixture<DnsServerFixture>
    {
    
        private const string TestDomain_V4 = "test.local";
        private const string TestIp_V4 = "192.168.1.100";

        private const string TestDomain_V6 = "test6.local";
        private const string TestIp_V6 = "fd00::100";

        private readonly DnsServerFixture _fixture = fixture;        


        [Fact]
        public async Task RegisterAndResolveDomain_ReturnsCorrectIPv4()
        {
            // Arrange: Register domain (assumes server is already running via fixture)
            string dns_ip = DnsConst.GetDnsIp();
            var dnsClient = new RestClient(dns_ip, DnsConst.ApiPort);
            await dnsClient.RegisterAsync(TestDomain_V4, TestIp_V4);
            // Act: Send DNS query
            var resolvedIp = await ClientUtils.SendDnsQueryIPv4Async(dns_ip, TestDomain_V4, DnsConst.UdpPort);

            // Assert
            Assert.Equal(TestIp_V4, resolvedIp);
        }

        [Fact]
        public async Task RegisterAndResolveDomain_ReturnsCorrectIPv6()
        {
            // Arrange: Register domain (assumes server is already running via fixture)
            string dns_ip = DnsConst.GetDnsIpV6();
            var dnsClient = new RestClient(dns_ip, DnsConst.ApiPort, useIPv6: true);
            await dnsClient.RegisterAsync(TestDomain_V6, TestIp_V6);

            // Act: Send DNS query (AAAA record)
            var resolvedIp = await ClientUtils.SendDnsQueryIPv6Async(dns_ip, TestDomain_V6, DnsConst.UdpPort);

            // Assert
            Assert.Equal(TestIp_V6.ToLowerInvariant(), resolvedIp.ToLowerInvariant());
        }
    }
    
}
