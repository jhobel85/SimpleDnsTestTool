using Xunit;
using DualstackDnsServer;

namespace SimpleDnsTests
{   
    public class DnsRecordMangerTest
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
