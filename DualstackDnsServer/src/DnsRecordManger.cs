using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

#nullable enable
namespace DualstackDnsServer;

public class DnsRecordManger : IDnsRecordManger
{
    private const string LogPrefix = "[DnsRecordManger]";
    private readonly ConcurrentDictionary<string, string> records = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>> sessions = new();
    private readonly object sessionLock = new();
    private readonly ILogger<DnsRecordManger> _logger;

    public DnsRecordManger(ILogger<DnsRecordManger>? logger = null)
    {
        _logger = logger ?? NullLogger<DnsRecordManger>.Instance;
        _logger.LogDebug("{Prefix} Server new instance", LogPrefix);
    }

    public void Register(string domain, string ip, string? sessionId = null)
    {
        records[domain] = ip;
        if (sessionId == null)
        {
            _logger.LogInformation("Register domain {Domain} ip {Ip}", domain, ip);
            return;
        }
        _logger.LogInformation("Register domain {Domain} ip {Ip} session {SessionId}", domain, ip, sessionId);
        AddSessionItem(sessionId, domain);
    }

    public void RegisterMany(IEnumerable<RestApi.DnsEntryDto> entries, string? sessionId = null)
    {
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Domain) || string.IsNullOrWhiteSpace(entry.Ip))
                continue;
            Register(entry.Domain, entry.Ip, sessionId);
        }
    }

    public void Unregister(string domain)
    {
        if (records.TryRemove(domain, out _))
        {
            _logger.LogInformation("Unregister domain {Domain}", domain);
        }
        else
        {
            _logger.LogDebug("{Prefix} Unregister domain {Domain} - not found", LogPrefix, domain);
        }
    }

    public string? Resolve(string domain)
    {
        // Use snapshot to avoid enumeration issues
        foreach (var kvp in records.ToArray())
        {
            string key = kvp.Key;
            if (domain.StartsWith(key, StringComparison.Ordinal))
            {
                string str = domain.Substring(key.Length);
                if (str.Length == 0 || str.StartsWith('.'))
                {
                    var recordType = kvp.Value?.Contains(':') == true ? "AAAA" : "A";
                    _logger.LogDebug("{Prefix} Resolve domain {Domain} -> {Ip} ({RecordType})", LogPrefix, domain, kvp.Value, recordType);
                    return kvp.Value;
                }
            }
        }
        _logger.LogDebug("{Prefix} Resolve domain {Domain} -> <not found>", LogPrefix, domain);
        return null;
    }

    public int GetCount() => this.records.Count;

    public int GetSessionCount(string sessionId)
    {
        return !sessions.TryGetValue(sessionId, out var value) ? 0 : value.Count;
    }

    public void UnregisterSession(string sessionId)
    {
        lock (sessionLock)
        {
            if (!sessions.TryGetValue(sessionId, out var value))
            {
                _logger.LogDebug("{Prefix} Unregister session {SessionId} - not found", LogPrefix, sessionId);
                return;
            }
            foreach (string key in value)
                records.TryRemove(key, out _);
            sessions.TryRemove(sessionId, out _);
            _logger.LogInformation("Unregister session {SessionId} and its domains", sessionId);
        }
    }

    public void UnregisterAll()
    {
        records.Clear();
        sessions.Clear();
        _logger.LogInformation("Unregister all domains and sessions");
    }

    private void AddSessionItem(string key, string domain)
    {
        sessions.AddOrUpdate(key, _ => [domain],
            (_, bag) => { bag.Add(domain); return bag; });
    }

    public IEnumerable<RestApi.DnsEntryDto> GetAllEntries()
    {
        return records.Select(kvp => new RestApi.DnsEntryDto { Domain = kvp.Key, Ip = kvp.Value }).ToList();
    }

}
