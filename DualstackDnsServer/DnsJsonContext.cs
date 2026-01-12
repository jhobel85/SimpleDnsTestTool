using System.Text.Json.Serialization;
using DualstackDnsServer.RestApi;
using System.Collections.Generic;

namespace DualstackDnsServer
{
    [JsonSerializable(typeof(List<DnsEntryDto>))]
    [JsonSerializable(typeof(List<DnsNameDto>))]
    public partial class DnsJsonContext : JsonSerializerContext
    {
    }
}