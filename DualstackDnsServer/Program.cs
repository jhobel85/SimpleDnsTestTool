
using Microsoft.AspNetCore.Server.Kestrel.Core;
using DualstackDnsServer;

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
            var urlList = new List<string>();
            if (DnsConst.IsHttpEnabled(config, args))
            {
                urlList.Add(DnsConst.ResolveHttpUrl(config));
                urlList.Add(DnsConst.ResolveHttpUrlV6(config));
            }
            urlList.Add(DnsConst.ResolveHttpsUrl(config));
            urlList.Add(DnsConst.ResolveHttpsUrlV6(config));
            var urls = urlList.ToArray();
            return GenericHostBuilderExtensions.ConfigureWebHostDefaults(Host.CreateDefaultBuilder(args), (Action<IWebHostBuilder>)(webBuilder =>
            {
                WebHostBuilderExtensions.UseStartup<Startup>(webBuilder);
                webBuilder.UseUrls(urls);
            }));
        }
    }
}


