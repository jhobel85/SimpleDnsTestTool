using Xunit;
using DualstackDnsServer.Utils;

namespace SimpleDnsTests
{
    public class DefaultServerManagerTest
    {
        [Fact]
        public void CanInstantiateDefaultServerManager()
        {
            var mgr = new DefaultServerManager();
            Assert.NotNull(mgr);
        }
    }
}
