using System;
using System.Collections.Generic;

#nullable enable
namespace SimpleDnsServer
{
    public class DnsRecordManger
    {
        private Dictionary<string, string> records = new Dictionary<string, string>();
        private Dictionary<string, List<string>> sessions = new Dictionary<string, List<string>>();

        public DnsRecordManger() => Console.WriteLine("Server new instance");

        public void Register(string domain, string ip, string? sessionId = null)
        {
            this.records[domain] = ip;
            if (sessionId == null)
                return;
            this.AddSessionItem(sessionId, domain);
        }

        public void Unregister(string domain) => this.records.Remove(domain);

        public string Resolve(string domain)
        {
            foreach (string key in this.records.Keys)
            {
                if (domain.StartsWith(key))
                {
                    string str = domain.Substring(key.Length);
                    if (str.Length == 0 || str.StartsWith("."))
                        return this.records[key];
                }
            }
            return (string)null;
        }

        public int GetCount() => this.records.Count;

        public int GetSessionCount(string sessionId)
        {
            return !this.sessions.ContainsKey(sessionId) ? 0 : this.sessions[sessionId].Count;
        }

        public void UnregisterSession(string sessionId)
        {
            if (!this.sessions.ContainsKey(sessionId))
                return;
            foreach (string key in this.sessions[sessionId])
                this.records.Remove(key);
            this.sessions.Remove(sessionId);
        }

        public void UnregisterAll() => this.records.Clear();

        private void AddSessionItem(string key, string domain)
        {
            if (this.sessions.ContainsKey(key))
                this.sessions[key].Add(domain);
            else
                this.sessions[key] = new List<string>() { domain };
        }
    }
}
