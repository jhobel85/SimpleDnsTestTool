using Xunit;
using SimpleDnsServer;

namespace SimpleDnsTests
{
    public class DnsRecordMangerTests
    {
        [Fact]
        public void CanRegisterAndResolveDomain()
        {
            var manager = new DnsRecordManger();
            manager.Register("example.com", "1.2.3.4");
            var ip = manager.Resolve("example.com");
            Assert.Equal("1.2.3.4", ip);
        }
    }
}
