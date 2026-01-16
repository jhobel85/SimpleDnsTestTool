using DnsClient;
using DualstackDnsServer;
using DualstackDnsServer.Services;


namespace DnsTests;

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
            string dns_ip = CliArgumentValidator.GetDnsIp();
            var httpClient = new RestClient(dns_ip, DnsConst.PortHttp);
            await httpClient.RegisterAsync(TestDomain_V4, TestIp_V4, true);
            // Act: Send DNS query
            var serverOptions = new ServerOptions
            {
                Ip = dns_ip,
                IpV6 = string.Empty,
                UdpPort = DnsConst.PortUdp
            };
            var udpClient = new DnsUdpClientService(serverOptions);
            var resolvedIp = await udpClient.QueryDnsIPv4Async(dns_ip, TestDomain_V4, DnsConst.PortUdp);

            // Assert
            Assert.Equal(TestIp_V4, resolvedIp);
        }

        [Fact]
        public async Task RegisterAndResolveDomain_ReturnsCorrectIPv6()
        {
            // Arrange: Register domain (assumes server is already running via fixture)
            string dns_ip = CliArgumentValidator.GetDnsIpV6();
            var httpClient = new RestClient(dns_ip, DnsConst.PortHttp);
            await httpClient.RegisterAsync(TestDomain_V6, TestIp_V6, true);

            // Act: Send DNS query (AAAA record)
            var serverOptions = new ServerOptions
            {
                Ip = string.Empty,
                IpV6 = dns_ip,
                UdpPort = DnsConst.PortUdp
            };
            var udpClient = new DnsUdpClientService(serverOptions);
            var resolvedIp = await udpClient.QueryDnsIPv6Async(dns_ip, TestDomain_V6, DnsConst.PortUdp);

            // Assert
            Assert.Equal(TestIp_V6.ToLowerInvariant(), resolvedIp.ToLowerInvariant());
        }
    }
