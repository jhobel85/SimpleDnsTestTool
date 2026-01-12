using DualstackDnsServer.Utils;
using Xunit;

namespace DualstackDnsServer.Tests
{
    public class DnsServerFixture : IDisposable
    {
        private readonly DefaultServerManager serverManager = new DefaultServerManager();
        private readonly DefaultProcessManager processManager = new DefaultProcessManager();

        public DnsServerFixture()
        {
            KillAnyRunningServer(); // before tests
            // Wait for ports to be free before starting
            WaitForPortToBeFree(DnsConst.UdpPort, DnsConst.GetDnsIp(), 20000);
            WaitForPortToBeFree(DnsConst.ApiHttp, DnsConst.GetDnsIp(), 20000);
            WaitForPortToBeFree(DnsConst.UdpPort, DnsConst.GetDnsIpV6(), 20000);
            WaitForPortToBeFree(DnsConst.ApiHttp, DnsConst.GetDnsIpV6(), 20000);
            // Always start with HTTP enabled for tests            
            serverManager.StartDnsServer(DnsConst.GetDnsIp(), DnsConst.GetDnsIpV6(), DnsConst.ApiHttp, DnsConst.UdpPort, true);
        }

private void KillAnyRunningServer()
{
    // Kill any DualstackDnsServer processes
    
    foreach (var proc in System.Diagnostics.Process.GetProcessesByName(DnsConst.DNS_SERVER_PROCESS_NAME))
    {
        try { proc.Kill(); proc.WaitForExit(); } catch { }
    }

    processManager.KillAllServers(DnsConst.UdpPort, DnsConst.GetDnsIp());
    processManager.KillAllServers(DnsConst.ApiHttp, DnsConst.GetDnsIp());
    processManager.KillAllServers(DnsConst.UdpPort, DnsConst.GetDnsIpV6());
    processManager.KillAllServers(DnsConst.ApiHttp, DnsConst.GetDnsIpV6());

    WaitForPortToBeFree(DnsConst.UdpPort, DnsConst.GetDnsIp(), 60000);
    WaitForPortToBeFree(DnsConst.ApiHttp, DnsConst.GetDnsIp(), 60000);
    WaitForPortToBeFree(DnsConst.UdpPort, DnsConst.GetDnsIpV6(), 60000);
    WaitForPortToBeFree(DnsConst.ApiHttp, DnsConst.GetDnsIpV6(), 60000);
}

        public void Dispose()
        {
            KillAnyRunningServer(); // after tests
            GC.SuppressFinalize(this);
        }

        // Wait for OS to release sockets (avoid port conflict)
        private void WaitForPortToBeFree(int port, string IP, int maxWaitMs = 10000, int pollMs = 250)
        {
            int waited = 0;
            while (processManager.IsServerRunning(port, IP))
            {
                if (waited >= maxWaitMs)
                    throw new TimeoutException($"Port {port} is still in use after waiting {maxWaitMs}ms");
                System.Threading.Thread.Sleep(pollMs);
                waited += pollMs;
            }
        }
    }
}
