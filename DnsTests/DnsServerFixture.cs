using DualstackDnsServer.Utils;
using Xunit;


namespace DualstackDnsServer;

    public class DnsServerFixture : IDisposable
    {
        private readonly ServerManager serverManager = new ServerManager();
        private readonly ProcessManager processManager = new ProcessManager();

        public DnsServerFixture()
        {
            KillAnyRunningServer(); // before tests
            // Wait for ports to be free before starting
            WaitForPortToBeFree(DnsConst.PortUdp, CliArgumentValidator.GetDnsIp(), 20000);
            WaitForPortToBeFree(DnsConst.PortHttp, CliArgumentValidator.GetDnsIp(), 20000);
            WaitForPortToBeFree(DnsConst.PortHttps, CliArgumentValidator.GetDnsIp(), 20000);
            WaitForPortToBeFree(DnsConst.PortUdp, CliArgumentValidator.GetDnsIpV6(), 20000);
            WaitForPortToBeFree(DnsConst.PortHttp, CliArgumentValidator.GetDnsIpV6(), 20000);
            WaitForPortToBeFree(DnsConst.PortHttps, CliArgumentValidator.GetDnsIpV6(), 20000);
            // Always start with HTTP enabled for tests            
            serverManager.StartDnsServer(CliArgumentValidator.GetDnsIp(), CliArgumentValidator.GetDnsIpV6(), DnsConst.PortHttps, DnsConst.PortHttp, DnsConst.PortUdp, true);
        }

private void KillAnyRunningServer()
{
    // Kill any DualstackDnsServer processes
    
    foreach (var proc in System.Diagnostics.Process.GetProcessesByName(DnsConst.DNS_SERVER_PROCESS_NAME))
    {
        try { proc.Kill(); proc.WaitForExit(); } catch { }
    }

    processManager.KillAllServers(DnsConst.PortUdp, CliArgumentValidator.GetDnsIp());
    processManager.KillAllServers(DnsConst.PortHttp, CliArgumentValidator.GetDnsIp());
    processManager.KillAllServers(DnsConst.PortHttps, CliArgumentValidator.GetDnsIp());
    processManager.KillAllServers(DnsConst.PortUdp, CliArgumentValidator.GetDnsIpV6());
    processManager.KillAllServers(DnsConst.PortHttp, CliArgumentValidator.GetDnsIpV6());
    processManager.KillAllServers(DnsConst.PortHttps, CliArgumentValidator.GetDnsIpV6());

    WaitForPortToBeFree(DnsConst.PortUdp, CliArgumentValidator.GetDnsIp(), 60000);
    WaitForPortToBeFree(DnsConst.PortHttp, CliArgumentValidator.GetDnsIp(), 60000);
    WaitForPortToBeFree(DnsConst.PortHttps, CliArgumentValidator.GetDnsIp(), 60000);
    WaitForPortToBeFree(DnsConst.PortUdp, CliArgumentValidator.GetDnsIpV6(), 60000);
    WaitForPortToBeFree(DnsConst.PortHttp, CliArgumentValidator.GetDnsIpV6(), 60000);
    WaitForPortToBeFree(DnsConst.PortHttps, CliArgumentValidator.GetDnsIpV6(), 60000);
}

        public void Dispose()
        {
            KillAnyRunningServer(); // after tests
            GC.SuppressFinalize(this);
        }

        // Wait for OS to release sockets (avoid port conflict)
        private void WaitForPortToBeFree(int port, string IP, int maxWaitMs = 10000, int pollMs = 500)
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
