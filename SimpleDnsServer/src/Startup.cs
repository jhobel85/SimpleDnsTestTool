#nullable enable
namespace SimpleDnsServer
{
    public class Startup
    {
        private IConfiguration configuration;

        public Startup(IConfiguration configuration) => this.configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            ServiceCollectionServiceExtensions.AddSingleton<DnsRecordManger>(services);
            MvcServiceCollectionExtensions.AddControllers(services);
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
}
