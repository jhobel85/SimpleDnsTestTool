using DualstackDnsServer;
using DualstackDnsServer.Client;
using DualstackDnsServer.Services;
using DualstackDnsServer.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading;

namespace DualstackDnsServer.RestApi;

[ApiController]
[Route("dns/query")]
public class DnsQueryController : ControllerBase
{
    private const string LogPrefix = "[DnsQueryController]";
    private readonly IDnsUdpClient _dnsUdpClient;
    private readonly ServerOptions? _serverOptions;
    private readonly ILogger<DnsQueryController> _logger;

    public DnsQueryController(IDnsUdpClient dnsUdpClient, ServerOptions? serverOptions = null)
    {
        _dnsUdpClient = dnsUdpClient;
        _serverOptions = serverOptions;
        // ILogger will be injected by DI
        _logger = (ILogger<DnsQueryController>?)AppDomain.CurrentDomain.GetData("DnsQueryControllerLogger");
    }
    
    /// <summary>
    /// Queries a DNS server for the specified domain using UDP (A or AAAA record).
    /// </summary>
    /// <param name="domain">Domain name to resolve.</param>
    /// <param name="type">Record type: AAAA or A.</param>
    /// <returns>Resolved IP address or error message.</returns>
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Query(string domain, string? type = null, CancellationToken cancellationToken = default)
    {
        domain = domain?.Trim().TrimEnd('.') ?? string.Empty;

        if (string.IsNullOrWhiteSpace(domain))
            return BadRequest("Domain is required.");

        // If DNS server is not configured, treat as BadRequest
        if (_serverOptions == null || (string.IsNullOrWhiteSpace(_serverOptions.Ip) && string.IsNullOrWhiteSpace(_serverOptions.IpV6)))
            return BadRequest("DNS server is required.");

        try
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                // Try both AAAA and A
                string serverForV6 = !string.IsNullOrWhiteSpace(_serverOptions.IpV6)
                    ? _serverOptions.IpV6
                    : _serverOptions.Ip;
                string serverForV4 = !string.IsNullOrWhiteSpace(_serverOptions.Ip)
                    ? _serverOptions.Ip
                    : _serverOptions.IpV6;

                string ipv6 = string.IsNullOrWhiteSpace(serverForV6)
                    ? string.Empty
                    : await _dnsUdpClient.QueryDnsAsync(serverForV6, domain, _serverOptions.UdpPort, QueryType.AAAA, cancellationToken);

                string ipv4 = string.IsNullOrWhiteSpace(serverForV4)
                    ? string.Empty
                    : await _dnsUdpClient.QueryDnsAsync(serverForV4, domain, _serverOptions.UdpPort, QueryType.A, cancellationToken);
                if (!string.IsNullOrWhiteSpace(ipv6) && !string.IsNullOrWhiteSpace(ipv4))
                    return Ok(new { IPv4 = ipv4, IPv6 = ipv6 });
                if (!string.IsNullOrWhiteSpace(ipv6))
                    return Ok(new { IPv6 = ipv6 });
                if (!string.IsNullOrWhiteSpace(ipv4))
                    return Ok(new { IPv4 = ipv4 });
                return NotFound($"No IP found for domain {domain}.");
            }
            else
            {
                QueryType qtype = type.ToUpper() == "AAAA" ? QueryType.AAAA : QueryType.A;
                string selectedServer = qtype == QueryType.AAAA
                    ? (!string.IsNullOrWhiteSpace(_serverOptions.IpV6) ? _serverOptions.IpV6 : _serverOptions.Ip)
                    : (!string.IsNullOrWhiteSpace(_serverOptions.Ip) ? _serverOptions.Ip : _serverOptions.IpV6);
                if (string.IsNullOrWhiteSpace(selectedServer))
                    return BadRequest($"DNS server for {qtype} is required.");

                string ip = await _dnsUdpClient.QueryDnsAsync(
                    selectedServer,
                    domain,
                    _serverOptions.UdpPort,
                    qtype,
                    cancellationToken);
                if (string.IsNullOrWhiteSpace(ip))
                    return NotFound($"No IP found for domain {domain}.");
                return Ok(qtype == QueryType.AAAA ? new { IPv6 = ip } : new { IPv4 = ip });
            }
        }
        catch (Exception ex)
        {
            // Return ObjectResult with 500 status code for test compatibility
            return new ObjectResult($"DNS query failed: {ex.Message}") { StatusCode = 500 };
        }
    }

        /// <summary>
    /// Queries a DNS server for the specified domain using UDP (A or AAAA record).
    /// </summary>
    /// <param name="domain">Domain name to resolve.</param>
    /// <param name="dnsServer">DNS server IP address (default: 8.8.8.8).</param>
    /// <param name="port">DNS server port (default: 53).</param>
    /// <param name="type">Record type: A or AAAA (default: A).</param>
    /// <returns>Resolved IP address or error message.</returns>
    [HttpGet("server")]
    public async Task<IActionResult> QueryWithServer(string domain, string? dnsServer = null, int? port = null, string? type = null, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("{Prefix} Entered Query action with domain={domain}, dnsServer={dnsServer}, port={port}, type={type}", LogPrefix, domain, dnsServer, port, type);
        domain = domain?.Trim().TrimEnd('.') ?? string.Empty;

        if (string.IsNullOrWhiteSpace(domain))
            return BadRequest("Domain is required.");

        string? effectiveServer = !string.IsNullOrWhiteSpace(dnsServer)
            ? dnsServer
            : (!string.IsNullOrWhiteSpace(_serverOptions?.IpV6) ? _serverOptions.IpV6 : _serverOptions?.Ip);
        int effectivePort = port ?? _serverOptions?.UdpPort ?? 53;

        if (string.IsNullOrWhiteSpace(effectiveServer))
            return BadRequest("DNS server is required.");

        try
        {
            _logger?.LogInformation("{Prefix} Before QueryDnsAsync", LogPrefix);
            if (string.IsNullOrWhiteSpace(type))
            {
                // Try both AAAA and A
                string ipv6 = await _dnsUdpClient.QueryDnsAsync(effectiveServer, domain, effectivePort, QueryType.AAAA, cancellationToken);
                string ipv4 = await _dnsUdpClient.QueryDnsAsync(effectiveServer, domain, effectivePort, QueryType.A, cancellationToken);
                if (!string.IsNullOrWhiteSpace(ipv6) && !string.IsNullOrWhiteSpace(ipv4))
                    return Ok(new { IPv4 = ipv4, IPv6 = ipv6 });
                if (!string.IsNullOrWhiteSpace(ipv6))
                    return Ok(new { IPv6 = ipv6 });
                if (!string.IsNullOrWhiteSpace(ipv4))
                    return Ok(new { IPv4 = ipv4 });
                return NotFound($"No IP found for domain {domain}.");
            }
            else
            {
                QueryType qtype = type.ToUpper() == "AAAA" ? QueryType.AAAA : QueryType.A;
                var queryTask = _dnsUdpClient.QueryDnsAsync(effectiveServer, domain, effectivePort, qtype, cancellationToken);
                _logger?.LogInformation("{Prefix} Awaiting QueryDnsAsync", LogPrefix);
                string ip = await queryTask;
                _logger?.LogInformation("{Prefix} After QueryDnsAsync, ip={ip}", LogPrefix, ip);
                if (string.IsNullOrWhiteSpace(ip))
                    return NotFound($"No IP found for domain {domain}.");
                return Ok(ip);
            }
        }
        catch (Exception ex)
        {
            var errorDetails = $"DNS query failed: {ex.Message}\nInner: {ex.InnerException?.Message}\nStack: {ex.StackTrace}";
            _logger?.LogError(ex, "{Prefix} {ErrorDetails}", LogPrefix, errorDetails);
            return StatusCode(500, errorDetails);
        }
    }    
}
