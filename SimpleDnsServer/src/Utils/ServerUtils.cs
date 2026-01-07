using System.Diagnostics;

namespace SimpleDnsServer.Utils
{
    public class ServerUtils
    {
        private static string GetServerExecutablePath()
        {            
            var testBinDir = AppDomain.CurrentDomain.BaseDirectory; //SimpleDnsTests/bin/Debug/net8.0            
            var solutionRoot = Directory.GetParent(testBinDir).Parent.Parent.Parent.Parent.FullName; // to solution root (SimpleDnsTestTool)
            string proc_name = Constants.DNS_SERVER_PROCESS_NAME + ".exe";
            var ret = Path.Combine(solutionRoot, "SimpleDnsServer", "bin", "Debug", Constants.FRAMEWORK, proc_name);
            if (!File.Exists(ret))
                throw new FileNotFoundException($"Could not find server executable at {ret}");
            return ret;
        }

        public static void StartDnsServer()
        {                   
            var serverExe = GetServerExecutablePath();
            StartDnsServer(serverExe, Constants.DNS_IP, Constants.ApiPort, Constants.UdpPort);
        }

        public static void StartDnsServer(String ip, int apiPort, int udpPort)
        {
            var serverExe = GetServerExecutablePath();
            StartDnsServer(serverExe, ip, apiPort, udpPort);
        }

        public static void StartDnsServer(string serverExe, String ip, int apiPort, int udpPort)
        {
            var _serverProcess = Process.Start(new ProcessStartInfo
            {
                FileName = serverExe,
                Arguments = $"--ip {ip} --apiPort {apiPort} --udpPort {udpPort}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
        
            // Wait for server to start or exit
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
}
