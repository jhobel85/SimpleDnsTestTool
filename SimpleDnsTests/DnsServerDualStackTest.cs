using System.Net;
using System.Threading.Tasks;
using Xunit;
using SimpleDnsClient;

namespace SimpleDnsServer.Tests
{    
    public class DnsServerDualStackTest(DnsServerFixture fixture) : IClassFixture<DnsServerFixture>
    {
        private const string TestDomain_V4 = "dualstack4.local";
        private const string TestIp_V4 = "192.168.1.101";
        private const string TestDomain_V6 = "dualstack6.local";
        private const string TestIp_V6 = "fd00::101";
        private readonly DnsServerFixture _fixture = fixture;  
        
        [Fact]
        public async Task RegisterAndResolve_BothIPv4AndIPv6_Success()
        {
            // Arrange
            string dns_ip_v4 = DnsConst.GetDnsIp(DnsIpMode.Localhost);
            string dns_ip_v6 = DnsConst.GetDnsIpV6(DnsIpMode.Localhost);
            var dnsClientV4 = new RestClient(dns_ip_v4, DnsConst.ApiPort);
            var dnsClientV6 = new RestClient(dns_ip_v6, DnsConst.ApiPort, useIPv6: true);

            // Register both records
            await dnsClientV4.RegisterAsync(TestDomain_V4, TestIp_V4);
            await dnsClientV6.RegisterAsync(TestDomain_V6, TestIp_V6);

            // Act
            var resolvedIpV4 = await ClientUtils.SendDnsQueryIPv4Async(dns_ip_v4, TestDomain_V4, DnsConst.UdpPort);
            var resolvedIpV6 = await ClientUtils.SendDnsQueryIPv6Async(dns_ip_v6, TestDomain_V6, DnsConst.UdpPort);

            // Assert
            Assert.Equal(TestIp_V4, resolvedIpV4);
            Assert.Equal(TestIp_V6.ToLowerInvariant(), resolvedIpV6.ToLowerInvariant());
        }

        [Fact]
        public async Task RegisterAndResolve_MultipleIPv4AndIPv6_Success()
        {            
            // Arrange
            var ipv4Domains = new[]
            {
                (domain: "multi4a.local", ip: "192.168.1.111"),
                (domain: "multi4b.local", ip: "192.168.1.112"),
                (domain: "multi4c.local", ip: "192.168.1.113"),
                (domain: "multi4d.local", ip: "192.168.1.114"),
                (domain: "multi4e.local", ip: "192.168.1.115"),
                (domain: "multi4f.local", ip: "192.168.1.116"),
                (domain: "multi4g.local", ip: "192.168.1.117")
            };
            var ipv6Domains = new[]
            {
                (domain: "multi6a.local", ip: "fd00::111"),
                (domain: "multi6b.local", ip: "fd00::112"),
                (domain: "multi6c.local", ip: "fd00::113"),
                (domain: "multi6d.local", ip: "fd00::114"),
                (domain: "multi6e.local", ip: "fd00::115"),
                (domain: "multi6f.local", ip: "fd00::116"),
                (domain: "multi6g.local", ip: "fd00::117")
            };

            string dns_ip_v4 = DnsConst.GetDnsIp(DnsIpMode.Localhost);
            string dns_ip_v6 = DnsConst.GetDnsIpV6(DnsIpMode.Localhost);
            var dnsClientV4 = new RestClient(dns_ip_v4, DnsConst.ApiPort);
            var dnsClientV6 = new RestClient(dns_ip_v6, DnsConst.ApiPort, useIPv6: true);

            // Register all records in parallel
            var registerTasks = ipv4Domains.Select(d => dnsClientV4.RegisterAsync(d.domain, d.ip))
                .Concat(ipv6Domains.Select(d => dnsClientV6.RegisterAsync(d.domain, d.ip)));
            await Task.WhenAll(registerTasks);

            try
            {
                // Act & Assert: Resolve all in parallel with a small delay between requests
                var v4Tasks = new List<Task>();
                foreach (var d in ipv4Domains)
                {
                    v4Tasks.Add(Task.Run(async () =>
                    {
                        var cts = new CancellationTokenSource(5000);
                        var resolvedIp = await ClientUtils.SendDnsQueryIPv4Async(dns_ip_v4, d.domain, DnsConst.UdpPort, cts.Token);
                        Assert.Equal(d.ip, resolvedIp);
                    }));
                    //await Task.Delay(1); // 1ms delay between requests
                }

                var v6Tasks = new List<Task>();
                foreach (var d in ipv6Domains)
                {
                    v6Tasks.Add(Task.Run(async () =>
                    {
                        var cts = new CancellationTokenSource(5000);
                        var resolvedIp = await ClientUtils.SendDnsQueryIPv6Async(dns_ip_v6, d.domain, DnsConst.UdpPort, cts.Token);
                        Assert.Equal(d.ip.ToLowerInvariant(), resolvedIp.ToLowerInvariant());
                    }));
                    //await Task.Delay(1); // 1ms delay between requests
                }

                await Task.WhenAll(v4Tasks.Concat(v6Tasks));
            }
            finally
            {
                // Cleanup: Unregister all test domains to avoid polluting server state
                // Optional feature - test should pass anyway                
                var unregisterTasks = ipv4Domains.Select(d =>
                {
                    try
                    {
                        return dnsClientV4.UnregisterAsync(d.domain).WaitAsync(TimeSpan.FromSeconds(5));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UnregisterAsync failed for {d.domain}: {ex.Message}");
                        return Task.CompletedTask;
                    }
                })
                .Concat(ipv6Domains.Select(d =>
                {
                    try
                    {
                        return dnsClientV6.UnregisterAsync(d.domain).WaitAsync(TimeSpan.FromSeconds(5));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UnregisterAsync failed for {d.domain}: {ex.Message}");
                        return Task.CompletedTask;
                    }
                }));
                try
                {
                    await Task.WhenAll(unregisterTasks);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unregister cleanup failed: {ex.Message}");
                }
                
            }
        }
    }
}
