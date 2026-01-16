# Start the server

1] Default run: **DualstackDnsServer.exe**

	- IPv4 will be localhost 127.0.0.1, UDP port 53 and API port 443 (HTTPS)
	- IPv6 will be localhost [::1], UDP port 53 and API port 443 (HTTPS)
	- HTTP disabled by default

2] Custom run (double-dash arguments only):

	**DualstackDnsServer.exe --ip 192.168.10.1 --ip6 fd00:10::1 --apiPort 8443 --udpPort 10053 --http true --cert "C:\mydns.local.pfx" --certPassw "P@ssw0rd!" --verbose true**

	- All arguments must use double dashes (e.g., --ip, --http, --verbose)
	- Custom IPv4 and IPv6 (**make sure custom IP address is registered on netowrk interface**)
	- Custom ports
	- Select certificate for custom IP
	- HTTP can be enabled by "--http true", will always run on port 80
	- If any parameters are not specified, default values will be used.

## See DNS packets in Wireshark 
The server resolves the name internally, without using DNS protocol. No UDP packets are created.

1] The server now also have possibility to query real DNS packets by **/dns/query** web api (see examples above).

2] Therefore, resolve via nslookup (works only with port 53):

```sh
nslookup mytest1234.test. 127.0.0.1
nslookup cpu30.local. 127.0.0.1

nslookup -q=AAAA ip6.com ::1
```

> Note: `nslookup ip6.com ::1` tries both IPv4 and IPv6 when you specify `::1`.

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

```sh
# Register
curl.exe -X POST "https://localhost:443/dns/register?domain=example.com&ip=fd00:10::20"

# Resolve
curl.exe -X GET "https://localhost:443/dns/resolve?domain=example.com"

# List all entries
curl.exe -g -X GET "http://[::1]:80/dns/entries"
```

### HTTP (if enabled with `--http true`)

```sh
curl.exe -X POST "http://127.0.0.1:80/dns/register?domain=example.com&ip=192.168.10.21"
curl.exe -X GET "http://127.0.0.1:80/dns/resolve?domain=example.com"
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

# AAAA record (IPv6)
nslookup -q=AAAA example.com ::1
```

> **Note:** The REST API resolves names internally without using the DNS protocol (no UDP packets). Use `nslookup` to test actual DNS protocol behavior.

---

