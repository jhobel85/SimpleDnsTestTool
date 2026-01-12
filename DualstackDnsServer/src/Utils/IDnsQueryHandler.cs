namespace DualstackDnsServer.Utils;

using ARSoft.Tools.Net.Dns;

public interface IDnsQueryHandler
{
    Task<DnsMessageBase?> HandleQueryAsync(DnsMessage query);
}
