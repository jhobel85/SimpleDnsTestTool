# SimpleDnsTestTool Overview

SimpleDnsTestTool is a dual-stack DNS server and client toolkit designed for testing and development. It supports both IPv4 and IPv6 traffic, allowing reliable DNS record registration, resolution, and unregistration. The project emphasizes test stability and reliability through async/await patterns, ensuring operations complete in order and tests run consistently across network stacks.

# DualstackDnsServer

Dual-stack means the server can handle both IPv4 and IPv6 traffic, ideally on the same port.

Reliable Registration: By awaiting async registration methods, you ensure that a DNS record is fully registered before sending a DNS query. This prevents timing issues where a query might be sent before the server is ready, which is especially important when running tests for both IPv4 and IPv6.

Test Stability: Async/await ensures that each step (register, resolve, unregister) completes in order, making your dual-stack tests pass consistently.

# How to start the server
1] Default run: DualstackDnsServer.exe
- IPv4 will be localhost 172.0.0.1, UDP port 53 and API port 44360 (HTTPS)
- IPv6 will be localhost [::1], UDP port 53 and API port 44360 (HTTPS)
- HTTP disabled by default
2] Custom run: DualstackDnsServer.exe --ip 192.168.50.1 --ip6 fd00:50::1 --apiPort 10053 --udpPort 10060 --http true
- custom IPv4 and IPv6
- custom ports
- HTTP can be enabled by "--http true", will always run on port 60
- If any of parameters not be specified default values be used.


# Rest API tests on localhost
## IPv4 Register and resovle:
curl -X POST "https://localhost:44360/dns/register?domain=ip4.com&ip=192.168.10.20"

curl -X GET "https://localhost:44360/dns/resolve?domain=ip4.com"

## IPv6 Register and resolve:
curl -g -X POST "https://localhost:44360/dns/register?domain=ip6.com&ip=fd00::101"

curl -g -X GET "https://localhost:44360/dns/resolve?domain=ip6.com"

## Show All entries
curl -g -X GET "https://localhost:44360/dns/entries"

## PowerShell syntax:
Invoke-WebRequest -Method POST "https://localhost:44360/dns/register?domain=ip6.com&ip=fd00::101"

Invoke-WebRequest -Uri "https://localhost:44360/dns/resolve?domain=ip6.com"

## See DNS packets in Wireshark
The server resolves the name internally, without using DNS protocol. No UDP packets are created.

Therfore Resolve via nslookup (work only with port 53):

nslookup ip4.com 127.0.0.1

nslookup -q=AAAA ip6.com ::1

(nslookup ip6.com ::1 // nslookup tries both IPv4 and IPv6 when you specify ::1)

# Architecture & Module Responsibilities

## Key Modules

- **IDnsRecordManger / DnsRecordManger**: Core DNS record storage and resolution logic. Registered as a singleton for both API and UDP listener.
- **IDnsQueryHandler / DefaultDnsQueryHandler**: Handles DNS protocol queries, decoupled from UDP listener for testability and extension.
- **IProcessManager / DefaultProcessManager**: Abstracts process management (find/kill/check server processes), replacing static ProcessUtils.
- **IServerManager / DefaultServerManager**: Abstracts server process startup, replacing static ServerUtils.
- **RestClient**: Client for API, now using IHttpClient abstraction for testability.
- **Startup.cs**: Registers all abstractions for dependency injection.

## Design Principles

- All infrastructure utilities are now injectable via interfaces, supporting testability and extension.
- Static helpers are marked obsolete and replaced by DI-registered services.
- UDP listener delegates DNS query handling to an injected handler, not direct record manager access.

## Extending/Testing

- To mock process/server/network logic, implement the relevant interface and register your mock in DI.
- For custom DNS query handling, implement IDnsQueryHandler and register it in Startup.cs.

---

# Enabling HTTPS for Local Development

To run the server and tests with HTTPS endpoints locally, you must trust the ASP.NET Core development certificate on your machine. This allows your server to listen on `https://localhost` without browser or client warnings.

## Steps to Enable HTTPS

1. **Trust the ASP.NET Core development certificate:**

	Open a terminal and run:

	```sh
	dotnet dev-certs https --trust
	```

	- On Windows, this will prompt you to trust the certificate. Click 'Yes' to confirm.
	- The certificate is stored in your user profile's certificate store (not as a file in your project).
	- You can view it in the Windows Certificate Manager under `Current User > Personal > Certificates` as `ASP.NET Core HTTPS development certificate` (certmgr.msc ).    

2. **Ensure your server is configured to listen on HTTPS:**

	The project is set up to listen on both HTTP and HTTPS by default. See `Properties/launchSettings.json` for the `applicationUrl` entry:

	```json
	"applicationUrl": "http://localhost:5144;https://localhost:7144"
	```

3. **Run the server:**

	You can now run the server and access it via both HTTP and HTTPS URLs.

4. **Testing with HTTPS:**
    You are using the same Windows user account as when you trusted the cert.

	When using tools like `curl`, add the `-k` flag to ignore certificate validation (for self-signed/dev certs):

	```sh
	curl -k https://localhost:7144/dns/resolve?domain=example.com
	```

---





