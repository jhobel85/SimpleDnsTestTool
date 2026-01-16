using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.Extensions.Logging;
using DualstackDnsServer.Services;

#nullable enable //compiler will warn if might be dereferencing a variable that could be null
namespace DualstackDnsServer;


public class DnsUdpListener : BackgroundService
{
    private readonly DnsServer udpServer;
    private readonly IDnsQueryHandler queryHandler;
    private static readonly SemaphoreSlim QuerySemaphore = new(500); // e.g., max 500 concurrent queries
    private readonly ILogger<DnsUdpListener> _logger;
    private readonly ServerOptions _serverOptions;

    public DnsUdpListener(IDnsQueryHandler queryHandler, IConfiguration config, ILogger<DnsUdpListener> logger, ServerOptions serverOptions)
    {
        _serverOptions = serverOptions;
        string ipString = string.IsNullOrWhiteSpace(_serverOptions.Ip) ? CliArgumentValidator.GetDnsIp() : _serverOptions.Ip;
        string ipStringV6 = string.IsNullOrWhiteSpace(_serverOptions.IpV6) ? string.Empty : _serverOptions.IpV6;
        int port = _serverOptions.UdpPort;
        this.queryHandler = queryHandler;
        _logger = logger;

        var transports = new List<UdpServerTransport>();

        if (!string.IsNullOrWhiteSpace(ipString))
        {
            var transportV4 = new UdpServerTransport(new IPEndPoint(IPAddress.Parse(ipString), port));
            SetUdpSocketBufferSafe(transportV4);
            transports.Add(transportV4);
        }

        if (!string.IsNullOrWhiteSpace(ipStringV6))
        {
            var transportV6 = new UdpServerTransport(new IPEndPoint(IPAddress.Parse(ipStringV6), port));
            SetUdpSocketBufferSafe(transportV6);
            transports.Add(transportV6);
        }

        if (transports.Count == 0)
        {
            throw new InvalidOperationException("No valid IP endpoints configured for DNS UDP listener.");
        }

        udpServer = new DnsServer(transports.ToArray());
        udpServer.QueryReceived += new AsyncEventHandler<QueryReceivedEventArgs>(OnQueryReceived);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011", Justification = "Reflection is required to tune ARSoft.Tools.Net UDP buffer; field is stable and access is safe in this context.")]
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
        {
            try
            {
                udpServer.Start();
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                // Determine which IP failed
                string failedIp = ex.Message.Contains(_serverOptions.IpV6) ? _serverOptions.IpV6 : _serverOptions.Ip;
                _logger.LogWarning($"The address '{failedIp}' is not assigned to any local network adapter. You may get a SocketException (10049). Use 'ipconfig' to see your assigned IPs.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not start UDP server: {ex.Message}");
            }
        }
        else
        {
            udpServer.Stop();
        }

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
