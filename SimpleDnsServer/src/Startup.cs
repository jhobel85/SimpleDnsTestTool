#nullable enable
namespace SimpleDnsServer;

public class Startup
{
    private IConfiguration configuration;

    public Startup(IConfiguration configuration) => this.configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
    #pragma warning disable IL2026
        services.AddSingleton<IDnsRecordManger, DnsRecordManger>();
    #pragma warning restore IL2026
    // Register System.Text.Json source generation context for DnsEntryDto
#pragma warning disable IL2026
    MvcServiceCollectionExtensions.AddControllers(services)
        .AddJsonOptions(options =>
        {
        options.JsonSerializerOptions.TypeInfoResolverChain.Add(SimpleDnsServer.DnsJsonContext.Default);
        });
#pragma warning restore IL2026
        ServiceCollectionHostedServiceExtensions.AddHostedService<DnsUdpListener>(services);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (HostEnvironmentEnvExtensions.IsDevelopment((IHostEnvironment)env))
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app);
        EndpointRoutingApplicationBuilderExtensions.UseRouting(app);
        EndpointRoutingApplicationBuilderExtensions.UseEndpoints(app, (Action<IEndpointRouteBuilder>)(endpoints => ControllerEndpointRouteBuilderExtensions.MapControllers(endpoints)));
    }
}
