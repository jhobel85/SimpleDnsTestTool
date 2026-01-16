# DnsTestTool Overview

DnsTestTool is a dual-stack DNS server and client toolkit designed for testing and development. It supports both IPv4 and IPv6 traffic, allowing reliable DNS record registration, resolution, and unregistration. The project emphasizes test stability and reliability through async/await patterns, ensuring operations complete in order and tests run consistently across network stacks.

---

# How to Start the Server

## Default Run

```sh
DualstackDnsServer.exe
```

- IPv4: `127.0.0.1`, UDP port `53`, HTTPS API port `443`
- IPv6: `[::1]`, UDP port `53`, HTTPS API port `443`
- HTTP disabled by default


## Run with custom IPv4 and http enabled

- Please make sure the IP address is present (has been added) on the netowrk interface

```sh
DualstackDnsServer.exe --ip 192.168.10.1 --http true
```

## Custom Run (double-dash arguments only)

```sh
DualstackDnsServer.exe --ip 192.168.10.1 --ip6 fd00:10::1 --apiPort 8443 --udpPort 10053 --http true --cert "C:\mydns.local.pfx" --certPassw "P@ssw0rd!" --verbose true
```

| Argument | Description | Default |
|----------|-------------|---------|
| `--ip` | IPv4 address to bind | `127.0.0.1` |
| `--ip6` | IPv6 address to bind | `::1` |
| `--apiPort` | HTTPS API port | `443` |
| `--udpPort` | DNS UDP port | `53` |
| `--http` | Enable HTTP on port 80 | `false` |
| `--cert` | Path to PFX certificate | _(dev cert)_ |
| `--certPassw` | Certificate password | _(none)_ |
| `--verbose` | Enable debug logging | `false` |

---

# REST API Operations

## Endpoints

| Operation | Method | Endpoint | Query Parameters / Body |
|-----------|--------|----------|-------------------------|
| **Register** | POST | `/dns/register` | `domain`, `ip` |
| **Register (Session)** | POST | `/dns/register/session` | `domain`, `ip`, `sessionId` |
| **Register Bulk** | POST | `/dns/register/bulk` | JSON body: `[{domain, ip}, ...]`, optional `sessionId` |
| **Unregister** | POST | `/dns/unregister` | `domain` |
| **Unregister Session** | POST | `/dns/unregister/session` | `sessionId` |
| **Unregister All** | DELETE | `/dns/unregister/all` | _(none)_ |
| **Resolve** | GET | `/dns/resolve` | `domain` |
| **List Entries** | GET | `/dns/entries` | _(none)_ |
| **Count** | GET | `/dns/count` | _(none)_ |
| **Count (Session)** | GET | `/dns/count/session` | `sessionId` |

---

# Architecture & Modules

![System Overview](doc/system_overview.png)

---

# Additional Documentation

- [Enabling HTTPS and Certificate handling on PC](doc/HTTPS_Certificates.md)
- [Test Examples](doc/TestExamples.md)
