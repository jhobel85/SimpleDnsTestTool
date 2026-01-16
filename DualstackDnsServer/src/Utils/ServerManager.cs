using System.Diagnostics;
using static DualstackDnsServer.DnsConst;

namespace DualstackDnsServer.Utils;

public class ServerManager : IServerManager
{
    public void StartDnsServer()
    {
        string root = GetSolutionRoot();
        var serverExe = GetServerExecutablePath(root);
        var ip = CliArgumentValidator.GetDnsIp(DnsIpMode.Localhost, null);
        var ip6 = CliArgumentValidator.GetDnsIpV6(DnsIpMode.Localhost, null);
        bool enabled = DEFAULT_ENABLE_HTTP;
        Console.WriteLine($"[DEBUG] DEFAULT_ENABLE_HTTP at test startup: {enabled}");
        StartDnsServer(serverExe, ip, ip6, PortHttps, PortHttp, PortUdp, enabled);
    }


    public void StartDnsServer(string ip, string ip6, int httpsPort, int httpPort, int udpPort, bool httpEnabled, string? cert = null, string? certPass = null)
    {
        string root = GetSolutionRoot();
        var serverExe = GetServerExecutablePath(root);
        StartDnsServer(serverExe, ip, ip6, httpsPort, httpPort, udpPort, httpEnabled, cert, certPass);
    }

    public void StartDnsServer(string serverExe, string ip, string ip6, int httpsPort, int httpPort, int udpPort, bool httpEnabled, string? cert = null, string? certPass = null)
    {
        var args = $"--ip {ip} --ip6 {ip6} --portHttps {httpsPort} --portHttp {httpPort} --udpPort {udpPort} --http {httpEnabled}";
        if (!string.IsNullOrEmpty(cert))
        {
            args += $" --cert {cert}";
        }
        if (!string.IsNullOrEmpty(certPass))
        {
            args += $" --certPass {certPass}";
        }
        var _serverProcess = Process.Start(new ProcessStartInfo
        {
            FileName = serverExe,
            Arguments = args,
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

    public static string GetServerExecutablePath(string solutionRoot)
    {        
        string proc_name = "DualstackDnsServer";
        var ret = Path.Combine(solutionRoot, proc_name, "bin", "Debug", "net8.0", proc_name + ".exe");
        if (!File.Exists(ret))
            throw new FileNotFoundException($"Could not find server executable at {ret}");
        return ret;
    }

    private static string GetSolutionRoot()
    {
        var testBinDir = AppDomain.CurrentDomain.BaseDirectory;
        var dir = new DirectoryInfo(testBinDir);
        for (int i = 0; i < 4; i++)
        {
            if (dir.Parent == null)
                throw new DirectoryNotFoundException($"Could not find solution root from testBinDir. Problem at: {dir.FullName}");
            dir = dir.Parent;
        }
        return dir.FullName;
    }
}
