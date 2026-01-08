# SimpleDnsTestTool Overview

SimpleDnsTestTool is a dual-stack DNS server and client toolkit designed for testing and development. It supports both IPv4 and IPv6 traffic, allowing reliable DNS record registration, resolution, and unregistration. The project emphasizes test stability and reliability through async/await patterns, ensuring operations complete in order and tests run consistently across network stacks.

# SimpleDnsServer

Definition: Dual-stack means the server can handle both IPv4 and IPv6 traffic, ideally on the same port.

Reliable Registration: By awaiting async registration methods, you ensure that a DNS record is fully registered before sending a DNS query. This prevents timing issues where a query might be sent before the server is ready, which is especially important when running tests for both IPv4 and IPv6.

Test Stability: Async/await ensures that each step (register, resolve, unregister) completes in order, making your dual-stack tests pass consistently.

# Rest API
Register:
curl -X POST "http://127.0.0.1:10060/dns/register?domain=example.com&ip=1.2.3.4"
Resolve:
curl -X POST "http://127.0.0.1:10060/dns/resolve?domain=example.com"







