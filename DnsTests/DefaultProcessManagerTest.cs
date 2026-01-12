using Xunit;
using DualstackDnsServer.Utils;

namespace SimpleDnsTests
{
    public class DefaultProcessManagerTest
    {
        [Fact]
        public void FindServerProcessIDs_ReturnsEmptySet_ForUnusedPort()
        {
            var mgr = new DefaultProcessManager();
            var result = mgr.FindServerProcessIDs(65534); // unlikely to be used
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void IsServerRunning_ReturnsFalse_ForUnusedPort()
        {
            var mgr = new DefaultProcessManager();
            Assert.False(mgr.IsServerRunning(65534));
        }
    }
}
