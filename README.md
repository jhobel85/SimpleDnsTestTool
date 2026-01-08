# SimpleDnsTestTool Overview

SimpleDnsTestTool is a dual-stack DNS server and client toolkit designed for testing and development. It supports both IPv4 and IPv6 traffic, allowing reliable DNS record registration, resolution, and unregistration. The project emphasizes test stability and reliability through async/await patterns, ensuring operations complete in order and tests run consistently across network stacks.

# SimpleDnsServer

Definition: Dual-stack means the server can handle both IPv4 and IPv6 traffic, ideally on the same port.

Reliable Registration: By awaiting async registration methods, you ensure that a DNS record is fully registered before sending a DNS query. This prevents timing issues where a query might be sent before the server is ready, which is especially important when running tests for both IPv4 and IPv6.

Test Stability: Async/await ensures that each step (register, resolve, unregister) completes in order, making your dual-stack tests pass consistently.

# How to start the server
1] run SimpleDnsServer.exe
- IPv4 will be localhost 172.0.0.1, UDP port 53 and API port 60
- IPv6 will be localhost [::1], UDP port 53 and API port 60
2] run SimpleDnsServer.exe --ip any --apiPort 10053 --udpPort 10060,
- IPv4 will be 0.0.0.0 with custom ports
- IPv6 will be [::] with custom ports
- With option "--ip localhost" - there can be defined localhost IPv4/IPv6 with custom ports.

# Rest API test
Register and resovle (IPv4):
curl -X POST "http://127.0.0.1:60/dns/register?domain=ip4.com&ip=1.2.3.4"
curl -X GET "http://127.0.0.1:60/dns/resolve?domain=ip4.com"

Register and resolve (IPv6):
curl -g -X POST "http://[::1]:60/dns/register?domain=ip6.com&ip=fd00::101"
curl -g -X GET "http://[::1]:60/dns/resolve?domain=ip6.com"

The server resolves the name internally, without using DNS protocol. No UDP packets are created.
Resolve via nslookup (work only with port 53):
nslookup ip4.com 127.0.0.1
nslookup -q=AAAA ip6.com ::1
(nslookup ip6.com ::1 // nslookup tries both IPv4 and IPv6 when you specify ::1)







