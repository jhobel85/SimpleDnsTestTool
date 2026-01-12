using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.Extensions.Logging;
using SimpleDnsServer.Utils;

#nullable enable //compiler will warn if might be dereferencing a variable that could be null
namespace SimpleDnsServer;


public class DnsUdpListener : BackgroundService
{
    private readonly DnsServer udpServer;
    private readonly IDnsQueryHandler queryHandler;
    private static readonly SemaphoreSlim QuerySemaphore = new(16); // e.g., max 16 concurrent queries
    private readonly ILogger<DnsUdpListener> _logger;

    public DnsUdpListener(IDnsQueryHandler queryHandler, IConfiguration config, ILogger<DnsUdpListener> logger)
    {
        string ipString = DnsConst.ResolveDnsIp(config);
        string ipStringV6 = DnsConst.ResolveDnsIpV6(config);
        int port = int.Parse(DnsConst.ResolveUdpPort(config));
        this.queryHandler = queryHandler;
        _logger = logger;

        // Best effort dual-stack: bind both IPv4 and IPv6 endpoints
        var transportV4 = new UdpServerTransport(new IPEndPoint(IPAddress.Parse(ipString), port));
        var transportV6 = new UdpServerTransport(new IPEndPoint(IPAddress.Parse(ipStringV6), port));

        SetUdpSocketBufferSafe(transportV4);
        SetUdpSocketBufferSafe(transportV6);

        udpServer = new DnsServer(transportV4, transportV6);
        udpServer.QueryReceived += new AsyncEventHandler<QueryReceivedEventArgs>(OnQueryReceived);
    }

    private void SetUdpSocketBufferSafe(UdpServerTransport transport)
    {
        try
        {
            // SonarQube S3011: Reflection is used here intentionally to access the private _udpClient field
            // in ARSoft.Tools.Net's UdpServerTransport for buffer tuning. This is safe because:
            // 1. The field name is stable in the library version used.
            // 2. Null checks and exception handling are in place.
            // 3. No sensitive data is exposed or modified.
            var socketField = typeof(UdpServerTransport).GetField("_udpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (socketField != null)
            {
                var udpClient = socketField.GetValue(transport) as UdpClient;
                udpClient?.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, DnsConst.UDP_BUFFER);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DnsUdpListener] Could not set UDP socket buffer size");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
            udpServer.Start();
        else
            udpServer.Stop();

        // Keep the background service alive until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task OnQueryReceived(object sender, QueryReceivedEventArgs e)
    {
        await QuerySemaphore.WaitAsync();
        try
        {
            if (e.Query is not DnsMessage query)
                return;
            try
            {
                var response = await queryHandler.HandleQueryAsync(query);
                e.Response = response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DnsUdpListener] Exception in query handler");
            }
        }
        finally
        {
            QuerySemaphore.Release();
        }
    }

    public override void Dispose()
    {
        udpServer?.Stop();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
