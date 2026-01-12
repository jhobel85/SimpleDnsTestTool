
using Microsoft.AspNetCore.Server.Kestrel.Core;
using SimpleDnsServer;

namespace SimpleDnsServer
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
            return GenericHostBuilderExtensions.ConfigureWebHostDefaults(Host.CreateDefaultBuilder(args), (Action<IWebHostBuilder>)(webBuilder =>
            {
                WebHostBuilderExtensions.UseStartup<Startup>(webBuilder);
                webBuilder.UseUrls(DnsConst.ResolveUrl(config), DnsConst.ResolveUrlV6(config));

            }));
        }
    }
}


