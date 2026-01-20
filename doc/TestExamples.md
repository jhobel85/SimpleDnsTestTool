# Start the server

## 1. Default run: `DualstackDnsServer.exe`

- **IPv4**: Defaults to `127.0.0.1` (localhost), UDP port 53, API port 443 (HTTPS)
- **IPv6**: *Not set by default*. To enable IPv6, you must specify the `--ip6` argument.
- **HTTP**: Disabled by default (only HTTPS is enabled unless `--http true` is specified)

## 2. Custom run (all arguments use double dashes)

Example:

```
DualstackDnsServer.exe --ip 192.168.10.1 --ip6 fd00:10::1 --portHttps 8443 --portHttp 8080 --portUdp 10053 --http true --cert "C:\mydns.local.pfx" --certPassw "P@ssw0rd!" --verbose true
```

- All arguments must use double dashes (e.g., `--ip`, `--http`, `--verbose`)
- **IPv4**: Set with `--ip` (must be registered on a network interface)
- **IPv6**: Set with `--ip6` (if omitted, IPv6 is not enabled)
- **Ports**: Set with `--portHttps` and `--portHttp` and `--portUdp`
- **Certificate**: Use `--cert` and `--certPassw` for custom HTTPS certs
- **HTTP**: Enable with `--http true` (always runs on port 80)
- If any parameters are not specified, default values will be used (except IPv6, which is disabled unless set)

> **Note:** If you do not specify `--ip6`, the server will not listen on IPv6.


## See DNS packets in Wireshark

The server resolves names internally via the REST API (no DNS protocol, no UDP packets). To see real DNS packets, use the `/dns/query` web API or test with `nslookup` (UDP port 53).

### Query via nslookup (UDP port 53 only)

```sh
nslookup mytest1234.test. 127.0.0.1
nslookup cpu30.local. 127.0.0.1

# For IPv6 (only if --ip6 was set and server is listening on IPv6):
nslookup -q=AAAA ip6.com ::1
```

> Note: `nslookup ip6.com ::1` tries both IPv4 and IPv6 when you specify `::1`. The server must be started with `--ip6 ::1` or another IPv6 address for this to work.

## Examples (curl)

### IPv4

```sh
# Register
curl.exe -X POST "https://localhost:443/dns/register?domain=example.com&ip=192.168.10.20"

# Resolve
curl.exe -X GET "https://localhost:443/dns/resolve?domain=example.com"

# Query
curl.exe -X GET "https://localhost:443/dns/query?domain=example.com"

# List all entries
curl.exe -X GET "https://localhost:443/dns/entries"

# Unregister
curl.exe -X POST "https://localhost:443/dns/unregister?domain=example.com"
```

### IPv6 

HTTP (if enabled with `--http true`)

```sh
# Register
curl.exe -X POST "https://localhost:443/dns/register?domain=example.com&ip=fd00:10::20"
curl.exe -X POST "http://192.168.0.242:80/dns/register?domain=cpu30ipv6.local&ip=fd00::30"

# Resolve
curl.exe -X GET "https://localhost:443/dns/resolve?domain=example.com"
curl.exe -X GET "http://192.168.0.242:80/dns/resolve?domain=cpu30ipv6.local"

# Query
curl.exe -X GET "http://192.168.0.242:80/dns/query?domain=cpu30ipv6.local."
curl.exe "http://192.168.0.242:80/dns/query/server?domain=cpu30ipv6.local&type=AAAA&dnsServer=192.168.0.242&port=53"

# List all entries (if HTTP enabled and IPv6 enabled)
curl.exe -g -X GET "http://[::1]:80/dns/entries"
curl.exe -g -X GET "http://192.168.0.242:80/dns/entries"
```

## Examples (PowerShell)

```powershell
# Register
Invoke-WebRequest -Method POST "https://localhost:443/dns/register?domain=example.com&ip=192.168.10.20" -UseBasicParsing

# Resolve
Invoke-WebRequest -Uri "https://localhost:443/dns/resolve?domain=example.com" -UseBasicParsing

# List entries
Invoke-WebRequest -Uri "https://localhost:443/dns/entries" -UseBasicParsing
```

---


# DNS Protocol (nslookup)

The DNS server also supports standard DNS protocol queries via UDP on port 53.

```sh
# A record (IPv4)
nslookup example.com 127.0.0.1

# AAAA record (IPv6, only if --ip6 is set)
nslookup -q=AAAA example.com ::1
```

> **Note:** The REST API resolves names internally without using the DNS protocol (no UDP packets). Use `nslookup` to test actual DNS protocol behavior. IPv6 queries require the server to be started with `--ip6`.

---

