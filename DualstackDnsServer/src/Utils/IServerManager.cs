namespace DualstackDnsServer.Utils;

public interface IServerManager
{
    void StartDnsServer();
    void StartDnsServer(string ip, string ip6, int apiPort, int udpPort, bool httpEnabled);
    void StartDnsServer(string serverExe, string ip, string ip6, int apiPort, int udpPort, bool httpEnabled);
}
