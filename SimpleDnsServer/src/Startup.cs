#nullable enable
namespace SimpleDnsServer;

public class Startup
{
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration) => this.configuration = configuration;

    public static void ConfigureServices(IServiceCollection services)
    {
    #pragma warning disable IL2026
#pragma warning disable IL2026
        services.AddSingleton<IDnsRecordManger, DnsRecordManger>();
        services.AddSingleton<Utils.IProcessManager, Utils.DefaultProcessManager>();
        services.AddSingleton<Utils.IServerManager, Utils.DefaultServerManager>();
        services.AddSingleton<Utils.IDnsQueryHandler, Utils.DefaultDnsQueryHandler>(sp =>
            new Utils.DefaultDnsQueryHandler(
                sp.GetRequiredService<IDnsRecordManger>(),
                sp.GetRequiredService<ILogger<Utils.DefaultDnsQueryHandler>>()
            )
        );
    #pragma warning restore IL2026
    // Register System.Text.Json source generation context for DnsEntryDto
#pragma warning disable IL2026
    MvcServiceCollectionExtensions.AddControllers(services)
        .AddJsonOptions(options =>
        {
        options.JsonSerializerOptions.TypeInfoResolverChain.Add(SimpleDnsServer.DnsJsonContext.Default);
        });
#pragma warning restore IL2026
        services.AddHostedService<DnsUdpListener>(sp =>
            new DnsUdpListener(
                sp.GetRequiredService<Utils.IDnsQueryHandler>(),
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
