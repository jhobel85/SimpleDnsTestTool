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
            ProcessUtils.KillAllServers(DnsConst.UdpPort, DnsConst.GetDnsIp());
            ProcessUtils.KillAllServers(DnsConst.ApiPort, DnsConst.GetDnsIp());
            ProcessUtils.KillAllServers(DnsConst.UdpPort, DnsConst.GetDnsIpV6());
            ProcessUtils.KillAllServers(DnsConst.ApiPort, DnsConst.GetDnsIpV6());

            WaitForPortToBeFree(DnsConst.UdpPort, DnsConst.GetDnsIp());
            WaitForPortToBeFree(DnsConst.ApiPort, DnsConst.GetDnsIp());
            WaitForPortToBeFree(DnsConst.UdpPort, DnsConst.GetDnsIpV6());
            WaitForPortToBeFree(DnsConst.ApiPort, DnsConst.GetDnsIpV6());

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
