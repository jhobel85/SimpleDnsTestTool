using Microsoft.AspNetCore.Server.Kestrel.Core;
using SimpleDnsServer;

// Entry point for running the server
CreateHostBuilder(args).Build().Run();

public partial class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        IConfigurationRoot config = CommandLineConfigurationExtensions.AddCommandLine((IConfigurationBuilder)new ConfigurationBuilder(), args).Build();        
        string port = DnsConst.ResolveApiPort(config);
        return GenericHostBuilderExtensions.ConfigureWebHostDefaults(Host.CreateDefaultBuilder(args), (Action<IWebHostBuilder>)(webBuilder =>
        {
            WebHostBuilderExtensions.UseStartup<Startup>(webBuilder);
            webBuilder.UseUrls(DnsConst.ResolveUrl(config), DnsConst.ResolveUrlV6(config));
            //WebHostBuilderKestrelExtensions.UseKestrel(webBuilder, (Action<KestrelServerOptions>)(options => options.ListenAnyIP(int.Parse(port))));
        }));
    }
}


