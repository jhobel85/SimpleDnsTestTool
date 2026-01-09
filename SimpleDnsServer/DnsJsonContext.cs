using System.Text.Json.Serialization;
using SimpleDnsServer.RestApi;
using System.Collections.Generic;

namespace SimpleDnsServer
{
    [JsonSerializable(typeof(List<DnsEntryDto>))]
    [JsonSerializable(typeof(List<DnsNameDto>))]
    public partial class DnsJsonContext : JsonSerializerContext
    {
    }
}