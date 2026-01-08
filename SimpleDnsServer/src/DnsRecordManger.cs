using System.Collections.Concurrent;

#nullable enable
namespace SimpleDnsServer;

public class DnsRecordManger : IDnsRecordManger
{
    private readonly ConcurrentDictionary<string, string> records = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>> sessions = new();
    private readonly object sessionLock = new();

    public DnsRecordManger() => Console.WriteLine("Server new instance");

    public void Register(string domain, string ip, string? sessionId = null)
    {
        records[domain] = ip;
        if (sessionId == null)
            return;
        AddSessionItem(sessionId, domain);
    }

    public void Unregister(string domain) => records.TryRemove(domain, out _);

    public string? Resolve(string domain)
    {
        // Use snapshot to avoid enumeration issues
        foreach (var kvp in records.ToArray())
        {
            string key = kvp.Key;
            if (domain.StartsWith(key))
            {
                string str = domain.Substring(key.Length);
                if (str.Length == 0 || str.StartsWith("."))
                    return kvp.Value;
            }
        }
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
                return;
            foreach (string key in value)
                records.TryRemove(key, out _);
            sessions.TryRemove(sessionId, out _);
        }
    }

    public void UnregisterAll()
    {
        records.Clear();
        sessions.Clear();
    }

    private void AddSessionItem(string key, string domain)
    {
        sessions.AddOrUpdate(key, _ => [domain],
            (_, bag) => { bag.Add(domain); return bag; });
    }
}
