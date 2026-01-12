using System.Diagnostics;
using static DualstackDnsServer.DnsConst;

namespace DualstackDnsServer.Utils;

public class DefaultServerManager : IServerManager
{
    private static string GetServerExecutablePath()
    {
        var testBinDir = AppDomain.CurrentDomain.BaseDirectory;
        var dir = new DirectoryInfo(testBinDir);
        for (int i = 0; i < 4; i++)
        {
            if (dir.Parent == null)
                throw new DirectoryNotFoundException($"Could not find solution root from testBinDir. Problem at: {dir.FullName}");
            dir = dir.Parent;
        }
        var solutionRoot = dir.FullName;
        string proc_name = DNS_SERVER_PROCESS_NAME + ".exe";
        var ret = Path.Combine(solutionRoot, "DualstackDnsServer", "bin", "Debug", FRAMEWORK, proc_name);
        if (!File.Exists(ret))
            throw new FileNotFoundException($"Could not find server executable at {ret}");
        return ret;
    }

    public void StartDnsServer()
    {
        var serverExe = GetServerExecutablePath();
        var ip = GetDnsIp(DnsIpMode.Localhost, null);
        var ip6 = GetDnsIpV6(DnsIpMode.Localhost, null);
        bool enabled = DEFAULT_ENABLE_HTTP;
        Console.WriteLine($"[DEBUG] DEFAULT_ENABLE_HTTP at test startup: {enabled}");
        StartDnsServer(serverExe, ip, ip6, ApiHttp, UdpPort, enabled);
    }

    public void StartDnsServer(string ip, string ip6, int apiPort, int udpPort, bool httpEnabled)
    {
        var serverExe = GetServerExecutablePath();
        StartDnsServer(serverExe, ip, ip6, apiPort, udpPort, httpEnabled);
    }

    public void StartDnsServer(string serverExe, string ip, string ip6, int apiPort, int udpPort, bool httpEnabled)
    {
        var _serverProcess = Process.Start(new ProcessStartInfo
        {
            FileName = serverExe,
            Arguments = $"--ip {ip} --ip6 {ip6} --apiPort {apiPort} --udpPort {udpPort} --http {httpEnabled}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });

        Thread.Sleep(3000);
        if (_serverProcess == null)
            throw new InvalidOperationException("Failed to start DNS server process.");

        if (_serverProcess.HasExited)
        {
            string stdOut = _serverProcess.StandardOutput.ReadToEnd();
            string stdErr = _serverProcess.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Server process exited early.\nStdOut: {stdOut}\nStdErr: {stdErr}");
        }

        Console.WriteLine("DNS Server succesfully started, process id " + _serverProcess.Id);
    }
}
