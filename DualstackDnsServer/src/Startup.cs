#nullable enable
using DualstackDnsServer.Services;

namespace DualstackDnsServer;

public class Startup
{
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration) => this.configuration = configuration;

    public static void ConfigureServices(IServiceCollection services)
    {
        // Register UDP DNS client service
        services.AddSingleton<IDnsUdpClientService, DnsUdpClientService>();
        services.AddSingleton<IDnsRecordManger, DnsRecordManger>();
        services.AddSingleton<Utils.IProcessManager, Utils.ProcessManager>();
        services.AddSingleton<Utils.IServerManager, Utils.ServerManager>();
        services.AddSingleton<IDnsQueryHandler, DnsQueryHandler>(sp =>
            new DnsQueryHandler(
                sp.GetRequiredService<IDnsRecordManger>(),
                sp.GetRequiredService<ILogger<DnsQueryHandler>>()
            )
        );
    #pragma warning disable IL2026
    // Register System.Text.Json source generation context for DnsEntryDto
    MvcServiceCollectionExtensions.AddControllers(services)
        .AddJsonOptions(options =>
        {
        options.JsonSerializerOptions.TypeInfoResolverChain.Add(DualstackDnsServer.DnsJsonContext.Default);
        options.JsonSerializerOptions.TypeInfoResolverChain.Add(new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver());
        });
#pragma warning restore IL2026
        services.AddHostedService<DnsUdpListener>(sp => new DnsUdpListener(
                sp.GetRequiredService<IDnsQueryHandler>(),
                sp.GetRequiredService<IConfiguration>(),
                sp.GetRequiredService<ILogger<DnsUdpListener>>()
            )
        );
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (HostEnvironmentEnvExtensions.IsDevelopment((IHostEnvironment)env))
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app);
        EndpointRoutingApplicationBuilderExtensions.UseRouting(app);
        EndpointRoutingApplicationBuilderExtensions.UseEndpoints(app, (Action<IEndpointRouteBuilder>)(endpoints => ControllerEndpointRouteBuilderExtensions.MapControllers(endpoints)));
    }
}
