
using Microsoft.AspNetCore.Server.Kestrel.Core;
using DualstackDnsServer;
using System.Net;

namespace DualstackDnsServer
{
    // Entry point for running the server
    public static class Program
    {
        public static void Main(string[] args)
        {            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {           
            IConfigurationRoot config = CommandLineConfigurationExtensions.AddCommandLine((IConfigurationBuilder)new ConfigurationBuilder(), args).Build();            

            // Validate CLI arguments and print help if needed
            CliArgumentValidator.ValidateArgs(args, DnsConst.SupportedKeys);
            // Parse all server options in one place
            var serverOptions = CliArgumentValidator.ParseServerOptions(config, args);
            
            var urlList = new List<string>();
            bool httpEnabled = serverOptions.EnableHttp;
            var httpsPort = serverOptions.HttpsPort;
            var httpPort = serverOptions.HttpPort;
            var ip = serverOptions.Ip;
            var ipV6 = serverOptions.IpV6;
            if (httpEnabled)
            {
                // HTTP URLs (for dev/testing)
                urlList.Add($"http://{ip}:{httpPort}");
                if (!string.IsNullOrWhiteSpace(ipV6))
                {
                    urlList.Add($"http://[{ipV6}]:{httpPort}");
                }
            }
            // HTTPS URLs (production/dev)
            urlList.Add($"https://{ip}:{httpsPort}");
            if (!string.IsNullOrWhiteSpace(ipV6))
            {
                urlList.Add($"https://[{ipV6}]:{httpsPort}");
            }
            var urls = urlList.ToArray();

            // Read certPath and certPassword from args or config
            string certPath = serverOptions.CertPath;
            string certPassword = serverOptions.CertPassword;

            var builder = Host.CreateDefaultBuilder(args);
                if (serverOptions.IsVerbose)
                {
                    builder.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                        logging.AddFilter((category, level) => level >= LogLevel.Debug);
                    });
                    Console.WriteLine("[INFO] Verbose logging enabled (Debug level)");
                }
            return GenericHostBuilderExtensions.ConfigureWebHostDefaults(builder, (Action<IWebHostBuilder>)(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(serverOptions);
                });
                WebHostBuilderExtensions.UseStartup<Startup>(webBuilder);
                webBuilder.ConfigureKestrel((context, kestrelOptions) =>
                {
                    foreach (var url in urls)
                    {
                        var uri = new Uri(url);
                        var port = uri.Port;
                        var host = uri.Host;
                        var scheme = uri.Scheme;

                        // Resolve host to IP(s); bind only to explicit IPs
                        var addresses = new List<System.Net.IPAddress>();
                        if (System.Net.IPAddress.TryParse(host, out var parsedIp))
                        {
                            addresses.Add(parsedIp);
                        }
                        else
                        {
                            try
                            {
                                addresses.AddRange(System.Net.Dns.GetHostAddresses(host));
                            }
                            catch
                            {
                                // Cannot resolve hostname; skip binding
                                continue;
                            }
                        }

                        foreach (var addr in addresses)
                        {
                            try
                            {
                                kestrelOptions.Listen(addr, port, listenOptions =>
                                {
                                    if (scheme == "https")
                                    {
                                        if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword))
                                        {
                                            listenOptions.UseHttps(certPath, certPassword);
                                        }
                                        else
                                        {
                                            listenOptions.UseHttps(); // fallback to default cert (e.g., dev cert)
                                        }
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[ERROR] Failed to bind {scheme.ToUpper()} endpoint {addr}:{port} - {ex.GetType().Name}: {ex.Message}");
                            }
                        }
                    }
                });
            }));
        }
    }
}


