using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.VisualBasic;
using SimpleDnsServer;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();
HostingAbstractionsHostExtensions.Run(CreateHostBuilder(args).Build());

static IHostBuilder CreateHostBuilder(string[] args)
{
    IConfigurationRoot config = CommandLineConfigurationExtensions.AddCommandLine((IConfigurationBuilder)new ConfigurationBuilder(), args).Build();
    string url = Constants.ResolveUrl(config);
    string port = Constants.ResolveApiPort(config);
    return GenericHostBuilderExtensions.ConfigureWebHostDefaults(Host.CreateDefaultBuilder(args), (Action<IWebHostBuilder>)(webBuilder =>
    {
        WebHostBuilderExtensions.UseStartup<Startup>(webBuilder);
        HostingAbstractionsWebHostBuilderExtensions.UseUrls(webBuilder, new string[1]
        {
        url
        });
        WebHostBuilderKestrelExtensions.UseKestrel(webBuilder, (Action<KestrelServerOptions>)(options => options.ListenAnyIP(int.Parse(port))));
    }));
}


