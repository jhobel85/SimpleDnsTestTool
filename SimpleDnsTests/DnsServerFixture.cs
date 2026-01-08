using SimpleDnsServer.Utils;
using Xunit;

namespace SimpleDnsServer.Tests
{
    public class DnsServerFixture : IDisposable
    {
        public DnsServerFixture()
        {
            KillAnyRunningServer(); // before tests
            ServerUtils.StartDnsServer();
        }

        private void KillAnyRunningServer()
        {
            ProcessUtils.KillAllServers(DnsConst.UdpPort, DnsConst.DNS_IP);
            ProcessUtils.KillAllServers(DnsConst.ApiPort, DnsConst.DNS_IP);
            ProcessUtils.KillAllServers(DnsConst.UdpPort, DnsConst.DNS_IPv6);
            ProcessUtils.KillAllServers(DnsConst.ApiPort, DnsConst.DNS_IPv6);

            WaitForPortToBeFree(DnsConst.UdpPort, DnsConst.DNS_IP);
            WaitForPortToBeFree(DnsConst.ApiPort, DnsConst.DNS_IP);
            WaitForPortToBeFree(DnsConst.UdpPort, DnsConst.DNS_IPv6);
            WaitForPortToBeFree(DnsConst.ApiPort, DnsConst.DNS_IPv6);

        }

        public void Dispose()
        {
            KillAnyRunningServer(); // after tests
        }

        // Wait for OS to release sockets (avoid port conflict)
        private void WaitForPortToBeFree(int port, string IP, int maxWaitMs = 10000, int pollMs = 250)
        {
            int waited = 0;
            while (ProcessUtils.IsServerRunning(port, IP))
            {
                if (waited >= maxWaitMs)
                    throw new TimeoutException($"Port {port} is still in use after waiting {maxWaitMs}ms");
                System.Threading.Thread.Sleep(pollMs);
                waited += pollMs;
            }
        }
    }
}
