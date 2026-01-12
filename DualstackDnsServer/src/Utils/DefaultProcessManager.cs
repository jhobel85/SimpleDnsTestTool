using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DualstackDnsServer.Utils;

public class DefaultProcessManager : IProcessManager
{
    public HashSet<int> FindServerProcessIDs(int portNr, string? ipAddress = null)
    {
        HashSet<int> result = [];
        string cmdArg = $"/C netstat -ano | findstr ':{portNr}";
        // Restrict PATH to fixed, unwriteable system directories
        //runs with a minimal, fixed, non-writable PATH
        string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System); // typically C:\Windows\System32
        string cmdFullPath = Path.Combine(system32, "cmd.exe");

        var startInfo = new ProcessStartInfo()
        {
            Arguments = cmdArg.Trim(),
            FileName = cmdFullPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        string cmdError = string.Empty;
        try
        {
            using Process cmd = new() { StartInfo = startInfo };
            cmd.Start();
            var stdOut = cmd.StandardOutput;
            var stdErr = cmd.StandardError;
            ProcessNetstatOutput(stdOut, portNr, ipAddress, result);
            cmdError = stdErr.ReadToEnd();
            cmd.WaitForExit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        if (!string.IsNullOrEmpty(cmdError))
            Console.WriteLine("Process returned error: " + cmdError);
        return result;
    }

    private static void ProcessNetstatOutput(System.IO.StreamReader stdOut, int portNr, string? ipAddress, HashSet<int> result)
    {
        while (!stdOut.EndOfStream)
        {
            var line = stdOut.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var splitResult = CleanAndSplitLine(line);
            if (splitResult.Length < 2)
                continue;
            TryAddProcessIdFromLine(splitResult, portNr, ipAddress, result);
        }
    }

    private static void TryAddProcessIdFromLine(string[] splitResult, int portNr, string? ipAddress, HashSet<int> result)
    {
        if (!TryParseIpAndPort(splitResult[1], out string? ip, out string? port))
            return;
        if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            return;
        if (!int.TryParse(port, out int foundPortNr))
            return;
        string normIp = ip;
        string? normArgIp = ipAddress;
        if (!string.IsNullOrEmpty(ipAddress))
        {
            try { normIp = System.Net.IPAddress.Parse(ip).ToString(); } catch { /* ignored: invalid IP */ }
            try { normArgIp = System.Net.IPAddress.Parse(ipAddress).ToString(); } catch { /* ignored: invalid IP */ }
        }
        if (foundPortNr == portNr && (ipAddress == null || normIp == normArgIp))
        {
            string strProcessNr = splitResult[^1];
            if (int.TryParse(strProcessNr, out int intProcessNr))
                result.Add(intProcessNr);
        }
    }

    private static string[] CleanAndSplitLine(string line)
    {
        // Replace multiple spaces with single space, then split
        while (line.Contains("  "))
            line = line.Replace("  ", " ");
        return line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool TryParseIpAndPort(string strIpAndPort, out string? ip, out string? port)
    {
        ip = null;
        port = null;
        int lastColon = strIpAndPort.LastIndexOf(':');
        if (lastColon > 0)
        {
            port = strIpAndPort[(lastColon + 1)..];
            ip = strIpAndPort[..lastColon];
            if (ip.StartsWith('[') && ip.EndsWith(']'))
                ip = ip[1..^1];
        }
        return ip != null && port != null;
    }

    private static bool KillProcessAsAdmin(int pid)
    {
        bool ret = true;
        string arg = @"/c taskkill /f" + " /pid " + pid;
        try
        {
            // Restrict PATH to fixed, unwriteable system directories
            string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System); // typically C:\Windows\System32
            string cmdFullPath = Path.Combine(system32, "cmd.exe");

            ProcessStartInfo processInf = new(cmdFullPath)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas",
                Arguments = arg
            };
            
            var proc = Process.Start(processInf);
            if (proc != null && proc.HasExited)
                Console.WriteLine("Process ID " + pid + " killed by admin rights.");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            ret = false;
        }
        return ret;
    }

    public bool KillAllServers(int portNr, string? ipAddress = null)
    {
        bool ret = true;
        foreach (var procId in FindServerProcessIDs(portNr, ipAddress))
            ret &= KillProcessAsAdmin(procId);
        return ret;
    }

    public bool IsServerRunning(int portNr, string? ipAddress = null)
    {
        var procesIds = FindServerProcessIDs(portNr, ipAddress);
        return procesIds.Count > 0;
    }
}
