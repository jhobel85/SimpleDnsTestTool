namespace DualstackDnsServer.Utils;

public interface IProcessManager
{
    HashSet<int> FindServerProcessIDs(int portNr, string? ipAddress = null);
    bool KillAllServers(int portNr, string? ipAddress = null);
    bool IsServerRunning(int portNr, string? ipAddress = null);
}
